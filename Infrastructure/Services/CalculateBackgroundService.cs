using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class CalculateBackgroundService : BackgroundService
    {
        private readonly INotificationService _notificationService;
        private readonly IQueueService _queueService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CalculateBackgroundService(
            INotificationService notificationService,
            IQueueService queueService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _notificationService = notificationService;
            _queueService = queueService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("CalculateBackgroundService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_queueService.TryDequeue(out int assetId))
                    {
                        using var scope = _serviceScopeFactory.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<AssetDbContext>();

                        // Check if any records exist first
                        var hasRecords = await dbContext.AssetInfos
                            .Where(ai => ai.AssetId == assetId)
                            .AnyAsync(stoppingToken);

                        if (!hasRecords)
                        {
                            Console.WriteLine($"No asset info found for AssetId: {assetId}");
                            continue;
                        }

                        double tempAvg = await dbContext.AssetInfos
                            .Where(ai => ai.AssetId == assetId)
                            .AverageAsync(ai => ai.Temperature, stoppingToken);

                        double powerAvg = await dbContext.AssetInfos
                            .Where(ai => ai.AssetId == assetId)
                            .AverageAsync(ai => ai.Power, stoppingToken);

                        await _notificationService.SendStatsToEveryone(tempAvg, powerAvg);

                        // Send notification through SignalR
                        //await _notificationService.SendCalculationResultAsync(assetId, tempAvg, powerAvg);
                    }
                    else
                    {
                        // No work → wait a little before retrying
                        await Task.Delay(500, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    Console.WriteLine("CalculateBackgroundService cancellation requested");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing background task: {ex.Message}");
                    // Wait a bit before continuing to avoid tight error loops
                    await Task.Delay(1000, stoppingToken);
                }
            }

            Console.WriteLine("CalculateBackgroundService stopped");
        }
    }
}
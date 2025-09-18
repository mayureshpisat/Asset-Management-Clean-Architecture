using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class CalculateBackgroundService : BackgroundService
    {
        private readonly INotificationService _notificationService;
        private readonly IQueueService _queueService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public CalculateBackgroundService(INotificationService notificationService, IQueueService queueService, IServiceScopeFactory serviceScopeFactory)
        {
            _notificationService = notificationService;
            _queueService = queueService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queueService.TryDequeue(out int assetId))
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AssetDbContext>();

                    var tempAvg = await dbContext.AssetInfos
                        .Where(ai => ai.AssetId == assetId)
                        .AverageAsync(ai => ai.Temperature, cancellationToken: stoppingToken);

                    var powerAvg = await dbContext.AssetInfos
                        .Where(ai => ai.AssetId == assetId)
                        .AverageAsync(ai => ai.Power, cancellationToken: stoppingToken);

                    Console.WriteLine($"FROM BACKGROUND METHOD {tempAvg} avg temp and {powerAvg} avg power");
                }
                else
                {
                    // No work → wait a little before retrying
                    await Task.Delay(500, stoppingToken);
                }
            }
        }

    }

}

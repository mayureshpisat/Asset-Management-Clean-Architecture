using Application.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundCalculatorService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IQueueService _queueService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private AsyncEventingBasicConsumer _consumer;
        private readonly HttpClient _httpClient;

        public Worker(
            ILogger<Worker> logger,
            IQueueService queueService,
            IServiceScopeFactory serviceScopeFactory,
            HttpClient httpClient)
        {
            _logger = logger;
            _queueService = queueService;
            _serviceScopeFactory = serviceScopeFactory;
            _httpClient = httpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundCalculatorService started at: {time}", DateTimeOffset.Now);
            _logger.LogInformation("CalculateBackgroundService started - Setting up consumer...");

            var channel = _queueService.GetChannel();
            _consumer = new AsyncEventingBasicConsumer(channel);

            _consumer.Received += async (sender, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("Received message: {message}", message);

                try
                {
                    if (int.TryParse(message, out int assetId))
                    {
                        await ProcessAssetAsync(assetId, stoppingToken);
                        channel.BasicAck(ea.DeliveryTag, multiple: false);
                        _logger.LogInformation("Message acknowledged for AssetId: {assetId}", assetId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to parse message: {message}", message);
                        channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            channel.BasicConsume(
                queue: "AssetProcessingQueue",
                autoAck: false,
                consumer: _consumer
            );

            _logger.LogInformation("Consumer setup complete - waiting for messages...");

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Service stopping...");
            }
        }

        private async Task ProcessAssetAsync(int assetId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing AssetId: {assetId}", assetId);

                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AssetDbContext>();

                var hasRecords = await dbContext.AssetInfos
                    .Where(ai => ai.AssetId == assetId)
                    .AnyAsync(cancellationToken);

                if (!hasRecords)
                {
                    _logger.LogWarning("No asset info found for AssetId: {assetId}", assetId);
                    return;
                }

                double tempAvg = await dbContext.AssetInfos
                    .Where(ai => ai.AssetId == assetId)
                    .AverageAsync(ai => ai.Temperature, cancellationToken);

                double powerAvg = await dbContext.AssetInfos
                    .Where(ai => ai.AssetId == assetId)
                    .AverageAsync(ai => ai.Power, cancellationToken);

                // Send stats to API endpoint (HTTP-based)
                var apiUrl = Environment.GetEnvironmentVariable("ASSETAPI_URL");
                var payload = new { Temperature = tempAvg, Power = powerAvg };
                await _httpClient.PostAsJsonAsync(apiUrl, payload);

                _logger.LogInformation(
                    "Successfully processed AssetId: {assetId} - TempAvg: {tempAvg}, PowerAvg: {powerAvg}",
                    assetId, tempAvg, powerAvg
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing asset {assetId}", assetId);
                throw;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BackgroundCalculatorService is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}


using Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Infrastructure.Services
{
    public class QueueService : IQueueService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string QueueName = "AssetProcessingQueue";

        public QueueService()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            Console.WriteLine("QueueService initialized and connected to RabbitMQ");
        }

        public void Enqueue(int assetId)
        {
            Console.WriteLine($"Enqueueing AssetId: {assetId}");

            var body = Encoding.UTF8.GetBytes(assetId.ToString());

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: "",
                routingKey: QueueName,
                basicProperties: properties,
                body: body
            );

            Console.WriteLine($"Successfully enqueued AssetId: {assetId}");
        }

        public IModel GetChannel()
        {
            return _channel;
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing QueueService...");
            _channel?.Close();
            _connection?.Close();
            Console.WriteLine("QueueService disposed");
        }
    }
}
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;
using System;
using Microsoft.AspNetCore.SignalR;

namespace StockRoomBlazor
{
    public class RabbitMQConsumer:BackgroundService
    {
        private readonly IHubContext<StockRoomSampleHub> _hubContext;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQConsumer(IHubContext<StockRoomSampleHub> hubContext)
        {
            _hubContext = hubContext;
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                //UserName = "guest",
                //Password= "password",

            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "messages", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _hubContext.Clients.All.SendAsync("Broadcast", message);
            };
            _channel.BasicConsume(queue: "messages", autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }
        public override void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
            base.Dispose();
        }
    }
}

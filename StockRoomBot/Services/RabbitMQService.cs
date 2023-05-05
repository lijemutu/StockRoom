using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace StockRoomBot.Services
{
    public class RabbitMQService:IRabbitMQService
    {
        protected readonly ConnectionFactory _factory;
        protected readonly IConnection _connection;
        protected readonly IModel _channel;
        protected readonly IServiceProvider _serviceProvider;

        public RabbitMQService(IServiceProvider serviceProvider)
        {
            _factory = new ConnectionFactory() { HostName="localhost"};
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _serviceProvider = serviceProvider;
        }

        public virtual void Connect()
        {
            // Declare a RabbitMQ Queue
            _channel.QueueDeclare(queue: "Broadcast", durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(_channel);

            // When we receive a message from SignalR
            consumer.Received += delegate (object model, BasicDeliverEventArgs ea) {
                // Get the ChatHub from SignalR (using DI)
                //var chatHub = (IHubContext<ChatHub>)_serviceProvider.GetService(typeof(IHubContext<ChatHub>));

                //// Send message to all users in SignalR
                //chatHub.Clients.All.SendAsync("messageReceived", "You have received a message");

            };

            // Consume a RabbitMQ Queue
            _channel.BasicConsume(queue: "Broadcast", autoAck: true, consumer: consumer);
        }
    }
}

using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace StockRoomBlazor
{
    public class RabbitMQServiceProducer:IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQServiceProducer()
        {
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
        public void SendMessage<T>(T message)
        {

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: "", routingKey: "messages", body: body);

        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}

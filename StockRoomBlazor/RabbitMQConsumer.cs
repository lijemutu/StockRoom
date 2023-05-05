using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;
using System;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http;
using System.Security.Policy;

namespace StockRoomBlazor
{
    public class RabbitMQConsumer:BackgroundService
    {
        private readonly IHubContext<StockRoomSampleHub> _hubContext;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly HttpClient _httpClient;
        public const string url = "https://localhost:7221/StockBot?stockName=";

        public RabbitMQConsumer(IHubContext<StockRoomSampleHub> hubContext, IHttpClientFactory httpClientFactory)
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
            _httpClient = httpClientFactory.CreateClient();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var rawMessage = Encoding.UTF8.GetString(body);
                rawMessage = rawMessage.Replace("\\\"", "\"").Trim('"');
                var user = rawMessage.Split(":")[0].Trim();
                var message = rawMessage.Split(":")[1].Trim();
                _hubContext.Clients.All.SendAsync("Broadcast", user, message);
                if (message.Contains("/stock"))
                {
                    var urlSent = url + message.Split("=")[1];
                    var response = _httpClient.GetAsync(urlSent).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseByte = response.Content.ReadAsByteArrayAsync().Result;
                        _channel.BasicPublish(exchange: "", routingKey: "messages",mandatory:false, body: responseByte);
                        //_hubContext.Clients.All.SendAsync("Broadcast", "StockBot",message);

                    }
                    else
                    {
                        throw new Exception($"Failed to get API content. Status code: {response.StatusCode}");
                    }
                }
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

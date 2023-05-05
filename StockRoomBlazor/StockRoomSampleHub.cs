using Microsoft.AspNetCore.SignalR;
using System.Security.Policy;
using static System.Net.Mime.MediaTypeNames;

namespace StockRoomBlazor
{
    public class StockRoomSampleHub:Hub
    {
        private readonly RabbitMQServiceProducer _rabbitMQService;
        public const string HubUrl = "/chatroom";


        public StockRoomSampleHub(RabbitMQServiceProducer rabbitMQService, HttpClient httpClient)
        {
            _rabbitMQService = rabbitMQService;
        }

        public async Task Broadcast(string username, string message)
        {
            var text = $"{username}: {message}";
            _rabbitMQService.SendMessage(text);
            //await Clients.All.SendAsync("Broadcast", username, message);

        }
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(e);
        }
    }
}

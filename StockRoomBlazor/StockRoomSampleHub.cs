using Microsoft.AspNetCore.SignalR;
using System.Security.Policy;
using static System.Net.Mime.MediaTypeNames;

namespace StockRoomBlazor
{
    public class StockRoomSampleHub:Hub
    {
        public const string HubUrl = "/chat";
        public const string url = "https://localhost:7221/StockBot?stockName=";
        private readonly RabbitMQServiceProducer _rabbitMQService;
        private readonly HttpClient _httpClient;


        public StockRoomSampleHub(RabbitMQServiceProducer rabbitMQService, HttpClient httpClient)
        {
            _rabbitMQService = rabbitMQService;
            _httpClient = httpClient;
        }

        public async Task Broadcast(string username, string message)
        {
            var text = $"{username}: {message}";
            _rabbitMQService.SendMessage(text);
            await Clients.All.SendAsync("Broadcast", username, message);
            if (message.Contains("/stock"))
            {
                var urlSent = url + message.Split("=")[1];
                var response = await _httpClient.GetAsync(urlSent);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    _rabbitMQService.SendMessage(responseString);
                    await Clients.All.SendAsync("Broadcast", "StockBot", responseString);

                }
                else
                {
                    throw new Exception($"Failed to get API content. Status code: {response.StatusCode}");
                }
            }

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

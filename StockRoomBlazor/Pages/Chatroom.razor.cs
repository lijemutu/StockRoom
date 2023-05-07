using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;

namespace StockRoomBlazor.Pages
{
    [Authorize(Policy = "LoggedIn")]
    public partial class Chatroom:ComponentBase
    {
        // flag to indicate chat status
        private bool _isChatting = false;

        // on-screen message
        private string _message;

        // new message input
        private string _newMessage;

        // list of messages in chat
        private List<Message> _messages = new List<Message>();
        private string _username;
        private string _hubUrl;
        private HubConnection _hubConnection;

        protected override async Task OnInitializedAsync()
        {
            // name of the user who will be chatting
            _username = _httpContextAccessor.HttpContext.User
                .FindFirst(ClaimTypes.GivenName).Value + _httpContextAccessor.HttpContext.User
                .FindFirst(ClaimTypes.Surname).Value;
            // check username is valid
            if (string.IsNullOrWhiteSpace(_username))
            {
                _message = "Please enter a name";
                return;
            };

            try
            {
                // Start chatting and force refresh UI.
                _isChatting = true;
                await Task.Delay(1);

                // remove old messages if any
                _messages.Clear();

                // Create the chat client
                string baseUrl = navigationManager.BaseUri;

                _hubUrl = baseUrl.TrimEnd('/') + StockRoomSampleHub.HubUrl;

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .Build();

                _hubConnection.On<string, string>("Broadcast", BroadcastMessage);

                await _hubConnection.StartAsync();

                await SendAsync($"[Notice] {_username} joined chat room.");
            }
            catch (Exception e)
            {
                _message = $"ERROR: Failed to start chat client: {e.Message}";
                _isChatting = false;
            }
        }

        private void BroadcastMessage(string name, string message)
        {
            bool isMine = name.Equals(_username, StringComparison.OrdinalIgnoreCase);

            if (_messages.Count > 50)
            {
                _messages.RemoveAt(0);
            }

            _messages.Add(new Message(name, message, isMine));

            // Inform blazor the UI needs updating
            InvokeAsync(StateHasChanged);
        }   

        private async Task DisconnectAsync()
        {
            if (_isChatting)
            {
                await SendAsync($"[Notice] {_username} left chat room.");

                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();

                _hubConnection = null;
                _isChatting = false;
            }
        }

        private async Task SendAsync(string message)
        {
            if (_isChatting && !string.IsNullOrWhiteSpace(message))
            {
                await _hubConnection.SendAsync("Broadcast", _username, message);

                _newMessage = string.Empty;
            }
        }

        private class Message
        {
            public Message(string username, string body, bool mine)
            {
                Username = username;
                Body = body;
                Mine = mine;
            }

            public string Username { get; set; }
            public string Body { get; set; }
            public bool Mine { get; set; }
            public DateTime TimeStamp { get; set; } = DateTime.Now;

            public bool IsNotice => Body.StartsWith("[Notice]");

            public string CSS => Mine ? "sent" : "received";
        }
    }

}

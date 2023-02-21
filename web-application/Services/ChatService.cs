using Microsoft.AspNetCore.SignalR.Client;
using server.Models;
using System.Threading.Tasks;
using System;

namespace web_application.Services
{
    public class ChatService
    {
        private readonly HubConnection _connection;

        public ChatService()
        {
            _connection = new HubConnectionBuilder().WithUrl("https://localhost:35944/chatsocket").Build();
        }

        public async Task StartAsync()
        {
            await _connection.StartAsync();
        }

        public async Task SendMessage(MessageDto messageDto)
        {
            await _connection.InvokeAsync("SendMessage", messageDto.user, messageDto.msgText);
        }

        public async Task ReceiveMessage(Action<MessageDto> receiveMessage)
        {
            _connection.On<string, string>("ReceiveOne", (user, message) =>
            {
                receiveMessage(new MessageDto
                {
                    user = user,
                    msgText = message
                });
            });
        }
    }
}

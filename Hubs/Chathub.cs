using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace ChatApp2.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(int senderId, int receiverId, string message, string messageType, string fileUrl)
        {
            await Clients.Group($"user_{receiverId}").SendAsync("ReceiveMessage", senderId, message, messageType, fileUrl);
        }

        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext().Session.GetInt32("UserId");
            if (userId != null)
            {
                Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception ex)
        {
            var userId = Context.GetHttpContext().Session.GetInt32("UserId");
            if (userId != null)
            {
                Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            return base.OnDisconnectedAsync(ex);
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using web_server.DbContext;

namespace web_server
{
    public class NotifHub : Hub
    {
        public static void SendNotification(string message, string to, IHubContext<NotifHub> hub)
        {
            var user = TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(to));

            var notif = new Notifications();
            var last = TestData.Notifications.LastOrDefault();
            notif.Id = last == null ? 0 : last.Id + 1;
            notif.Message = message;
            notif.UserIdTo = Convert.ToInt32(to);
            notif.DateTime = DateTime.Now;

            TestData.Notifications.Add(notif);

            var connecedTokens = user.NotificationTokens.Tokens.Where(m => m.Value == "Connected");
            foreach (var item in connecedTokens)
            {
                hub.Clients.Client(item.Key).SendAsync("ReceiveNotification", message, false, notif.Id);
            }
        }

        public async Task SetNotifications(int userId)
        {
            var notifs = TestData.Notifications.Where(m => m.UserIdTo == userId).ToList();
            foreach (var item in notifs)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveNotification", item.Message, item.Readed, item.Id);
            }
        }
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var ctx = Context.GetHttpContext();
            if (!ctx.Request.Query.ContainsKey("token"))
            {
                return;
            }
            var userId = Convert.ToInt32(ctx.Request.Query["token"]);

            var user = TestData.UserList.FirstOrDefault(m => m.UserId == userId);
            if (!user.NotificationTokens.Tokens.ContainsKey(connectionId))
            {
                TestData.UserList.FirstOrDefault(m => m.UserId == userId).NotificationTokens.Tokens.Add(connectionId, "Connected");
            }
            else
            {
                TestData.UserList.FirstOrDefault(m => m.UserId == userId).NotificationTokens.Tokens[connectionId] = "Connected";
            }

            await SetNotifications(userId);
        }

        public override Task OnDisconnectedAsync(Exception ex)
        {
            var connectionId = Context.ConnectionId;
            var userId = Convert.ToInt32(Context.GetHttpContext().Request.Query["token"]);

            TestData.UserList.FirstOrDefault(m => m.UserId == userId).NotificationTokens.Tokens[connectionId] = "Disconnected";

            return Task.CompletedTask;
        }

    }
}

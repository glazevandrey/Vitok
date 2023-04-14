using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;

namespace web_server
{
    public class NotifHub : Hub
    {
        UserRepository _userRepository;
        public NotifHub(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async static void SendNotification(string message, string to, IHubContext<NotifHub> hub, UserRepository userRepository, NotificationRepository notificationRepository)
        {
            if (to == "-1")
            {
                return;
            }

            var user = await userRepository.GetUserById(Convert.ToInt32(to));
            if(user == null)
            {
                return;
            }

            var notif = new Notifications();
            notif.Message = message;
            notif.UserIdTo = Convert.ToInt32(to);
            notif.DateTime = DateTime.Now;
            user.Notifications.Add(notif);

            try
            {
                await userRepository.Update(user);

            }
            catch (Exception ex)
            {

                throw ex;
            }

            try
            {
                var connecedTokens = user.NotificationTokens.Where(m => m.TokenValue == "Connected").ToList();
                foreach (var item in connecedTokens)
                {
                    await hub.Clients.Client(item.TokenKey).SendAsync("ReceiveNotification", message, false, notif.Id);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task SetNotifications(int userId)
        {
            var notifs = await _userRepository.GetUserNotifications(userId);
            //var notifs = TestData.Notifications.Where(m => m.UserIdTo == userId).ToList();
            notifs.Reverse();
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
            var user =  await _userRepository.GetUserById(userId);
           // var user = TestData.UserList.FirstOrDefault(m => m.UserId == userId);
            if (user == null)
            {
                return;
            }
            if (user.NotificationTokens.FirstOrDefault(m => m.TokenKey == connectionId) == null)
            {
                await _userRepository.AddTonificationTokenToUser(new NotificationTokens() { TokenKey = connectionId, TokenValue = "Connected" }, userId);
                //TestData.UserList.FirstOrDefault(m => m.UserId == userId).NotificationTokens.Add(new NotificationTokens() { TokenKey = connectionId, TokenValue = "Connected" });
            }
            else
            {

                await _userRepository.ChangeNotifTokenStatus("Connected", connectionId, userId);
                //TestData.UserList.FirstOrDefault(m => m.UserId == userId).NotificationTokens.FirstOrDefault(m => m.TokenKey == connectionId).TokenValue = "Connected";
            }

            await SetNotifications(userId);
        }
        public async override Task OnDisconnectedAsync(Exception ex)
        {
            var connectionId = Context.ConnectionId;
            var userId = Convert.ToInt32(Context.GetHttpContext().Request.Query["token"]);
            
                await _userRepository.ChangeNotifTokenStatus("Disconnected", connectionId, userId);

           

            //TestData.UserList.FirstOrDefault(m => m.UserId == userId).NotificationTokens.FirstOrDefault(m => m.TokenKey == connectionId).TokenValue = "Disconnected";
        }

    }
}

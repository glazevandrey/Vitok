using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server
{
    public class NotifHub : Hub
    {
        UserRepository _userRepository;
        IMapper _mapper;
        public NotifHub(IMapper mapper,UserRepository userRepository)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public async static Task SendNotification(string message, string to, IHubContext<NotifHub> hub, UserRepository userRepository, NotificationRepository notificationRepository, IMapper mapper)
        {
            if (to == "-1" || to == "1")
            {
                return;
            }

            var user = await userRepository.GetUser(Convert.ToInt32(to));
            if(user == null)
            {
                return;
            }

            var notif = new Notifications();
            notif.Message = message;
            notif.UserIdTo = Convert.ToInt32(to);
            notif.DateTime = DateTime.Now;
            user.Notifications.Add(mapper.Map<NotificationsDTO>(notif));

            try
            {
                await userRepository.SaveChanges(user);

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
        public async Task SetNotifications(List<NotificationsDTO> notifs)
        {

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
            var user =  await _userRepository.GetUser(userId);
           // var user = TestData.UserList.FirstOrDefault(m => m.UserId == userId);
            if (user == null)
            {
                return;
            }
            if (user.NotificationTokens.FirstOrDefault(m => m.TokenKey == connectionId) == null)
            {
                await _userRepository.AddTonificationTokenToUser(new NotificationTokens() { TokenKey = connectionId, TokenValue = "Connected" }, user);
                //TestData.UserList.FirstOrDefault(m => m.UserId == userId).NotificationTokens.Add(new NotificationTokens() { TokenKey = connectionId, TokenValue = "Connected" });
            }
            

            await SetNotifications(user.Notifications);
        }
        public async override Task OnDisconnectedAsync(Exception ex)
        {
            var connectionId = Context.ConnectionId;
            var userId = Convert.ToInt32(Context.GetHttpContext().Request.Query["token"]);
            var user = await _userRepository.GetUser(userId);
           var rem = user.NotificationTokens.FirstOrDefault(m=>m.TokenKey == connectionId);
            user.NotificationTokens.Remove(rem);
            await _userRepository.SaveChanges(user);
           // await _userRepository.ChangeNotifTokenStatus("Disconnected", connectionId, user);

           

            //TestData.UserList.FirstOrDefault(m => m.UserId == userId).NotificationTokens.FirstOrDefault(m => m.TokenKey == connectionId).TokenValue = "Disconnected";
        }

    }
}

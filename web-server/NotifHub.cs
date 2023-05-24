using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Database;
using web_server.Database.Repositories;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server
{
    public class NotifHub : Hub
    {
        UserRepository _userRepository;
        IMapper _mapper;
        private static IServiceProvider _serviceProvider;
        public NotifHub(IMapper mapper, UserRepository userRepository)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }
        public static void Configure(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        //        public async static Task SendNotification(string message, string to, IHubContext<NotifHub> hub, UserRepository userRepository, IMapper mapper)

        public async static Task SendNotification(string message, string to, IHubContext<NotifHub> hub, IMapper mapper)
        {
            if (to == "-1" || to == "1")
            {
                return;
            }
            var scope = _serviceProvider.CreateScope();

            DataContext db = scope.ServiceProvider.GetRequiredService<DataContext>();
            var user = await db.Users.Include(m => m.NotificationTokens).Include(m => m.Notifications).FirstOrDefaultAsync(m => m.UserId == Convert.ToInt32(to));

            if (user == null)
            {
                return;
            }

            var notif = new Notifications();
            notif.Message = message;
            notif.UserIdTo = Convert.ToInt32(to);
            notif.DateTime = DateTime.Now;
            
            var mapped = mapper.Map<NotificationsDTO>(notif);
            user.Notifications.Add(mapped);
            try
            {
                await db.SaveChangesAsync();
                db.Entry(user).State = EntityState.Detached;
                db.Dispose();

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
                    await hub.Clients.Client(item.TokenKey).SendAsync("ReceiveNotification", message, false, mapped.Id);
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
            var user = await _userRepository.GetUser(userId);

            if (user == null)
            {
                return;
            }

            if (user.NotificationTokens.FirstOrDefault(m => m.TokenKey == connectionId) == null)
            {
                await _userRepository.AddTonificationTokenToUser(new NotificationTokens() { TokenKey = connectionId, TokenValue = "Connected" }, user);
            }

            user.Notifications = user.Notifications.OrderBy(m => m.DateTime).ToList();
            await SetNotifications(user.Notifications);
        }
        public async override Task OnDisconnectedAsync(Exception ex)
        {
            var connectionId = Context.ConnectionId;
            await _userRepository.RemoveNotificationToken(connectionId);
        }

    }
}

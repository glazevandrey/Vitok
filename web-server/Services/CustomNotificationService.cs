using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class CustomNotificationService : ICustomNotificationService
    {
        UserRepository _userRepository;
        NotificationRepository _notificationRepository;
        IHubContext<NotifHub> _hubContext;
        public CustomNotificationService(NotificationRepository notificationRepository, UserRepository userRepository, IHubContext<NotifHub> hubContext )
        {
            
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }
        public async Task SendMessage(string message, string to)
        {
            await NotifHub.SendNotification(message, to, _hubContext, _userRepository, _notificationRepository);
        }
    }
}

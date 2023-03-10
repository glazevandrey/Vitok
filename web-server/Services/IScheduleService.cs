using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using web_server.Models;

namespace web_server.Services
{
    public interface IScheduleService
    {
        public Schedule AddScheduleFromUser(string args, IHubContext<NotifHub> _hubContext);
        public List<Schedule> GetSchedules(string args);
        public Schedule ChangeStatus(string args, IHubContext<NotifHub> _hubContext);
    }
}

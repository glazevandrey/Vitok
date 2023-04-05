using Microsoft.AspNetCore.SignalR;
using web_server.Models;

namespace web_server.Services.Interfaces
{
    public interface ITutorService
    {
        public User AddTutor(string args);
        public User UpdateTutor(string args);
        public bool RemoveTutor(string args);
        public User AddTutorFreeDate(string args);
        public User AddTutorSchedule(string args, IHubContext<NotifHub> _hubContext);
        public User RemoveTutorSchedule(string args, IHubContext<NotifHub> _hubContext);
        public User RemoveTutorTime(string args);
        public bool RejectStudent(string[] args, IHubContext<NotifHub> _hubContext);


    }
}

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using web_server.Models.DBModels;

namespace web_server.Services.Interfaces
{
    public interface ILessonsService
    {
        public Task<User> AddLessonsToUser(string[] args);
        //public Task<List<RescheduledLessons>> GetRescheduledLessons(string args);
        public Task<Schedule> RescheduleLesson(string args, IHubContext<NotifHub> _hubContext);

    }
}

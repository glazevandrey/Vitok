using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using web_server.Models.DBModels;
using web_server.Models.V2;

namespace web_server.Services.Interfaces
{
    public interface ILessonsService
    {
        public Task<User> AddLessonsToUser(string[] args);
        //public Task<List<RescheduledLessons>> GetRescheduledLessons(string args);
        public Task<Schedule> RescheduleLesson(string args, IHubContext<NotifHub> _hubContext);
        public Task<Schedule> RescheduleLesson(RescheduleLessonDto request, IHubContext<NotifHub> _hubContext);

    }
}

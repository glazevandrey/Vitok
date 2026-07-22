using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using web_server.Models.DBModels;
using web_server.Models.V2;

namespace web_server.Services.Interfaces
{
    public interface ITutorService
    {
        public Task<Tutor> AddTutor(string args);
        public Task<Tutor> UpdateTutor(string args);
        public Task<bool> RemoveTutor(string args);
        public Task<Tutor> AddTutorFreeDate(string args);
        public Task<List<Tutor>> GetAll();
        public Task<Tutor> AddTutorSchedule(string args, IHubContext<NotifHub> _hubContext);
        public Task<Tutor> RemoveTutorSchedule(string args, IHubContext<NotifHub> _hubContext);
        public Task RemoveTutorScheduleV2(RemoveLessonRequest request, IHubContext<NotifHub> _hubContext);
        public Task<Tutor> GetTutor(string args);
        public Task<bool> RejectStudent(string[] args, IHubContext<NotifHub> _hubContext);


    }
}

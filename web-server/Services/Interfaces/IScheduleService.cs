using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using web_server.Models;
using web_server.Models.DBModels;

namespace web_server.Services.Interfaces
{
    public interface IScheduleService
    {
        public Task<Schedule> AddScheduleFromUser(string args, IHubContext<NotifHub> _hubContext);
        public Task<List<Schedule>> GetSchedules(string args);
        public Task<List<Schedule>> GetAllSchedules();
        public Task<List<RescheduledLessons>> GetAllReschedules();
        public Task<Schedule> GetScheduleById(int id);
        public Task<bool> Update(Schedule schedule);

        public Task<Schedule> UpdateRange(List<Schedule> schedules); 
        public Task<string> ChangeStatus(string args, IHubContext<NotifHub> _hubContext);
    }
}

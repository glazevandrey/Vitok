using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server.Services.Interfaces
{
    public interface IScheduleService
    {
        public Task<Schedule> AddScheduleFromUser(string args, IHubContext<NotifHub> _hubContext);
        public Task<List<ScheduleDTO>> GetSchedules(string args);
        public Task<List<ScheduleDTO>> GetAllSchedules();
        public Task<List<RescheduledLessons>> GetAllReschedules();
        public Task<ScheduleDTO> GetScheduleById(int id);
        public Task<bool> Update(ScheduleDTO schedule);
        public Task<string> ChangeStatus(string args, IHubContext<NotifHub> _hubContext);
    }
}

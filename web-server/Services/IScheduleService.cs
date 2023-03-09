using System.Collections.Generic;
using web_server.Models;

namespace web_server.Services
{
    public interface IScheduleService
    {
        public Schedule AddScheduleFromUser(string args);
        public List<Schedule> GetSchedules(string args);
    }
}

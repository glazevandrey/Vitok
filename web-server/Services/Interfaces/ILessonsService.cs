using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using web_server.Models;
using web_server.Models.DBModels;

namespace web_server.Services.Interfaces
{
    public interface ILessonsService
    {
        public User AddLessonsToUser(string[] args);
        public List<RescheduledLessons> GetRescheduledLessons(string args);
        public Schedule RescheduleLesson(string args, IHubContext<NotifHub> _hubContext);

    }
}

using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using web_server.DbContext;
using web_server.Models;

namespace web_server.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IHubContext<NotifHub> _hubContext;
        public ScheduleService(IHubContext<NotifHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public Schedule AddScheduleFromUser(string args)
        {
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Registration>(args);
            var user = TestData.UserList.FirstOrDefault(m => m.UserId == model.UserId);
           
            var schedule = TestData.Schedules.FirstOrDefault(m => m.StartDate.DayOfWeek == model.WantThis.dateTimes[0].DayOfWeek && m.StartDate.ToString("HH:mm") == model.WantThis.dateTimes[0].ToString("HH:mm"));
            schedule.Course = model.Course;
            schedule.UserId = model.UserId;
            schedule.Status = user.LessonsCount == 0 ? Status.ОжидаетОплату : Status.Ожидает;
            schedule.UserName = user.FirstName + " " + user.LastName;
            schedule.CreatedDate = DateTime.Now;

            var text = Constatnts.NOTIF_NEW_LESSON_TUTOR.Replace("{name}", user.FirstName + " " + user.LastName).Replace("{date}", schedule.StartDate.ToString("dd.MM.yyyy HH:mm"));

            // отправка репетитору что у новое занятие
            NotifHub.SendNotification(text, model.TutorId.ToString(), _hubContext);

            return schedule;
           
        }

        public List<Schedule> GetSchedules(string args)
        {
            var user = TestData.UserList.FirstOrDefault(m => m.ActiveToken == args);
            if (user == null)
            {
                return null;
            }

            var schedules = TestData.Schedules.Where(m => m.UserId == user.UserId).ToList();
            if (schedules == null || schedules.Count == 0)
            {
                schedules = TestData.Schedules.Where(m => m.TutorId == user.UserId).ToList();

            }
            return schedules;
        }
    }
}

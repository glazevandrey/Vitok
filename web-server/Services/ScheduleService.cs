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

        public Schedule ChangeStatus(string args)
        {
            var split = args.Split(';');
            var status = split[0];
            var tutor_id = Convert.ToInt32(split[1]);
            var user_id = Convert.ToInt32(split[2]);
            var date = DateTime.Parse(split[3]);
            var dateCurr = DateTime.Parse(split[4]);

            var model = TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == date);
            var user = TestData.UserList.FirstOrDefault(m => m.UserId == user_id);
            var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == tutor_id);
            var schedule = TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == date);

            if ((Status)Enum.Parse(typeof(Status), status) == Status.Проведен)
            {
                user.LessonsCount--;
                user.BalanceHistory.CustomMessages.Add(DateTime.Now, "-1 занятие");


                tutor.Balance += 1000;
                tutor.BalanceHistory.CashFlow.Add(new CashFlow() { Date = DateTime.Now, Amount = 1000 });

                schedule.Tasks[Constatnts.NOTIF_START_LESSON] = false;
                schedule.Tasks[Constatnts.NOTIF_TOMORROW_LESSON] = false;
                schedule.Tasks[Constatnts.NOTIF_DONT_FORGET_SET_STATUS] = false;

                if (user.LessonsCount == 1)
                {
                    NotifHub.SendNotification(Constatnts.NOTIF_ONE_LESSON_LEFT, user_id.ToString(), _hubContext);
                }
                else if (user.LessonsCount == 0)
                {
                    NotifHub.SendNotification(Constatnts.NOTIF_ZERO_LESSONS_LEFT, user_id.ToString(), _hubContext);
                    NotifHub.SendNotification(Constatnts.NOTIF_ZERO_LESSONS_LEFT_FOR_MANAGER.Replace("{name}",
                        user.FirstName + " " + TestData.UserList.FirstOrDefault(m => m.UserId == user_id).LastName), TestData.Managers.First().UserId.ToString(), _hubContext);
                }

            }
            if (model.Looped)
            {
                schedule.ReadyDates.Add(dateCurr);
            }
            else
            {
                schedule.Status = (Status)Enum.Parse(typeof(Status), status);
            }

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

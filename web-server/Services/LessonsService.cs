using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using web_server.DbContext;
using web_server.Models;

namespace web_server.Services
{
    public class LessonsService : ILessonsService
    {
        public List<RescheduledLessons> GetRescheduledLessons(string args)
        {
            var user = TestData.UserList.FirstOrDefault(m => m.ActiveToken == args);
            if (user == null)
            {
                return null;
            }

            var schedules = new List<RescheduledLessons>();
            if (user.Role == "Tutor")
            {
                schedules = TestData.RescheduledLessons.Where(m => m.TutorId == user.UserId).ToList();
            }
            else
            {
                schedules = TestData.RescheduledLessons.Where(m => m.UserId == user.UserId).ToList();
            }

            if (schedules == null || schedules.Count == 0)
            {
                schedules = TestData.RescheduledLessons.Where(m => m.TutorId == user.UserId).ToList();
            }

            return schedules;
        }
        public User AddLessonsToUser(string[] args)
        {

            var user = TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(args[0]));
            user.LessonsCount += Convert.ToInt32(args[1]);

            var schedules = TestData.Schedules.Where(m => m.UserId == user.UserId).ToList();
            var isTrial = Convert.ToBoolean(args[2]);
            if (isTrial)
            {
                TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).UsedTrial = true;
            }
            var lessonCount = Convert.ToInt32(args[1]);
            var tariff = TestData.Tariffs.FirstOrDefault(m => m.LessonsCount == lessonCount);
            if (tariff != null)
            {
                TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).BalanceHistory.CustomMessages.Add(DateTime.Now, $"Оплата тарифа: {tariff.Title}");
            }
            else
            {
                TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).BalanceHistory.CustomMessages.Add(DateTime.Now, $"Оплачено занятий: {lessonCount}");
            }

            for (int i = 0; i < Convert.ToInt32(args[1]); i++)
            {
                var waited = schedules.Where(m => m.Status == Status.ОжидаетОплату).ToList();
                if (waited.Count > 0)
                {
                    schedules.FirstOrDefault(m => m.Id == waited.First().Id).Status = Status.Ожидает;
                    user.LessonsCount -= 1;
                }
            }

            return user;
        }

        public Schedule RescheduleLesson(string args, IHubContext<NotifHub> _hubContext)
        {
            var split = args.Split(';');

            //    status = 0
            //    tut = 1
            //    dateOld = 2
            //    loop = 3
            //    user = 4
            //    newdate = 5
            //    reason = 6
            //    initiator = 7
            //    newTime = 8
            //    courseId = 9

            var oldDateTime = DateTime.Parse(split[2]);
            var newDateTime = DateTime.Parse(split[5] + " " + split[8]);
            var status = split[0];

            var tutor_id = Convert.ToInt32(split[1]);
            var user_id = Convert.ToInt32(split[4]);
            var loop = split[3];
            var initiator = split[7];
            var reason = split[6];
            var courseId = Convert.ToInt32(split[9]);
            var cureDate = DateTime.Parse(split[10]);
            var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == tutor_id);
            var user = TestData.UserList.FirstOrDefault(m => m.UserId == user_id);

            // отправка студенту что перенос занятия
            NotifHub.SendNotification(Constants.NOTIF_LESSON_WAS_RESCHEDULED_FOR_STUDENT
                .Replace("{name}", tutor.FirstName + " " + tutor.LastName)
                .Replace("{date}", newDateTime.ToString("dd.MM.yyyy HH:mm")), user_id.ToString(), _hubContext);

            if (Convert.ToBoolean(loop))
            {
                var new_model = new Schedule
                {
                    TutorId = tutor_id,
                    UserId = user_id,
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    UserName = user.FirstName + " " + user.LastName,
                    Course = TestData.Courses.FirstOrDefault(m => m.Id == courseId),
                    Date = new UserDate() { dateTimes = new List<DateTime>() { newDateTime } },
                    Id = TestData.Schedules.Last().Id + 1,
                    StartDate = newDateTime,
                    Looped = true,
                };


                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).Status = Status.Перенесен;
                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).RescheduledId = new_model.Id;
                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).RescheduledDate = cureDate;

                TestData.Schedules.Add(new_model);

                // отправка манагеру что постоянный перенос
                NotifHub.SendNotification(Constants.NOTIF_REGULAR_RESCHEDULE.Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName)
                    .Replace("{studentName}", user.FirstName + " " + user.LastName)
                    .Replace("{oldDate}", oldDateTime.ToString("dd.MM.yyyy HH:mm"))
                    .Replace("{newDate}", cureDate.ToString("dd.MM.yyyy HH:mm")), TestData.Managers.First().UserId.ToString(), _hubContext);

                return new_model;
            }
            else
            {
                var model = new RescheduledLessons
                {
                    Initiator = initiator,
                    NewTime = newDateTime,
                    OldTime = oldDateTime,
                    Reason = reason,
                    TutorId = tutor_id,
                    UserId = user_id
                };

                var new_model = new Schedule
                {
                    TutorId = tutor_id,
                    UserId = user_id,
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    UserName = user.FirstName + " " + user.LastName,
                    Course = TestData.Courses.FirstOrDefault(m => m.Id == courseId),
                    Date = new UserDate() { dateTimes = new List<DateTime>() { newDateTime } },
                    Id = TestData.Schedules.Last().Id + 1,
                    StartDate = newDateTime,
                    Looped = false,
                };

                var re_less = new RescheduledLessons() { Initiator = initiator, NewTime = newDateTime, OldTime = cureDate, Reason = reason, TutorId = tutor_id, UserId = user_id };

                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).Status = Status.Перенесен;

                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).RescheduledLessons.Add(re_less);

                TestData.Schedules.Add(new_model);

                return new_model;
            }
        }
    }
}

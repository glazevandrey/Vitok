using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using web_server.DbContext;
using web_server.Models;

namespace web_server.Services
{
    public class TutorService : ITutorService
    {
        public User AddTutor(string args)
        {
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(args.ToString());
            model.Id = TestData.UserList.Last().Id + 1;
            model.UserId = TestData.UserList.Last().UserId + 1;

            TestData.UserList.Add(model);

            return model;
        }

        public User AddTutorFreeDate(string args)
        {
            var split = args.Split(';');
            var tutor_id = split[0];
            var dateTime = DateTime.Parse(split[1]);
            var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id));
            if (tutor != null)
            {
                TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id)).UserDates.dateTimes.Add(dateTime);
                TestData.Schedules.Add(new Schedule()
                {
                    Id = TestData.Schedules.Last().Id + 1,
                    Date = new UserDate() { dateTimes = new List<DateTime>() { dateTime } },
                    Looped = Convert.ToBoolean(split[2]),
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    TutorId = tutor.Id,
                    UserId = -1,
                    StartDate = dateTime,
                });

            }

            return tutor;
        }

        public User AddTutorSchedule(string args, IHubContext<NotifHub> _hubContext)
        {
            var split = args.Split(';');
            var tutor_id = split[0];
            var user_id = split[3];
            var course_id = split[4];

            var dateTime = DateTime.Parse(split[1]);
            var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id));
            var user = TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(user_id));
            var course = TestData.Courses.FirstOrDefault(m => m.Id == Convert.ToInt32(course_id));
            if (tutor != null)
            {
                tutor.UserDates.dateTimes.Add(dateTime);
                TestData.Schedules.Add(new Schedule()
                {
                    Id = TestData.Schedules.Last().Id + 1,
                    Date = new UserDate() { dateTimes = new List<DateTime>() { dateTime } },
                    UserName = user.FirstName + " " + user.LastName,
                    Looped = Convert.ToBoolean(split[2]),
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    TutorId = tutor.Id,
                    UserId = user.UserId,
                    Course = course,
                    Status = Status.Ожидает,
                    StartDate = dateTime
                });

                // отправка манагеру что новый урок у репетитора
                NotifHub.SendNotification(Constants.NOTIF_NEW_LESSON.Replace("{studentName}", user.FirstName + " " + user.LastName)
                    .Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateTime.ToString("dd.MM.yyyy HH:mm")), user_id.ToString(), _hubContext);
            }

            return tutor;
        }

        public bool RemoveTutor(string args)
        {
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(args);
            if (user != null)
            {
                TestData.Tutors.RemoveAll(m => m.UserId == user.UserId);
                if (TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId) != null)
                {
                    TestData.UserList.RemoveAll(m => m.UserId == user.UserId);
                    return true;
                }
            }

            return false;
        }

        public User RemoveTutorSchedule(string args, IHubContext<NotifHub> _hubContext)
        {

            var split = args.Split(';');
            var dateTime = DateTime.Parse(split[2]);
            var tutor_id = split[0];
            var user_id = split[1];
            var curr = DateTime.Parse(split[3]);
            var manager_id = TestData.Managers.First().UserId;
            var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id));
            if (tutor != null)
            {
                tutor.UserDates.dateTimes.Remove(dateTime);
                var schedule = TestData.Schedules.FirstOrDefault(m => m.TutorId == Convert.ToInt32(tutor_id) && m.Date.dateTimes[0] == dateTime && m.UserId == Convert.ToInt32(user_id));
                if (schedule != null)
                {
                    schedule.RemoveDate = curr;
                }

                // отправка манагеру что удалено занятие
                NotifHub.SendNotification(Constants.NOTIF_REMOVE_LESSON.Replace("{tutorName}", schedule.TutorFullName).Replace("{studentName}", schedule.UserName).Replace("{date}", dateTime.ToString("dd.MM.yyyy HH:mm")), manager_id.ToString(), _hubContext);
            }

            return tutor;
        }

        public User RemoveTutorTime(string args)
        {
            var split = args.Split(';');
            var tutor_id = split[0];
            var dateTime = DateTime.Parse(split[1]);
            var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id));
            if (tutor != null)
            {
                TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id)).UserDates.dateTimes.Remove(dateTime);
            }

            return tutor;
        }

        public User UpdateTutor(string args)
        {
            var tutor = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(args);
            var old = TestData.UserList.FirstOrDefault(m => m.UserId == tutor.UserId);

            old.FirstName = tutor.FirstName;
            old.LastName = tutor.LastName;
            old.MiddleName = tutor.MiddleName;
            old.BirthDate = tutor.BirthDate;
            old.About = tutor.About;
            old.Email = tutor.Email;
            old.Wish = tutor.Wish;
            old.Courses = tutor.Courses;
            old.Password = tutor.Password;
            old.Phone = tutor.Phone;

            return tutor;
        }

    }
}

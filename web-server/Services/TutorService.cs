using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using web_server.DbContext;
using web_server.Models;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class TutorService : ITutorService
    {
        public User AddTutor(string args)
        {
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(args.ToString());
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
                    TutorId = tutor.UserId,
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
            if (user.Credit.Where(m => m.Repaid == false).ToList().Count >= 3)
            {
                return null;
            }

            if (tutor != null)
            {
                var sch = new Schedule()
                {
                    Id = TestData.Schedules.Last().Id + 1,
                    Date = new UserDate() { dateTimes = new List<DateTime>() { dateTime } },
                    UserName = user.FirstName + " " + user.LastName,
                    Looped = Convert.ToBoolean(split[2]),
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    TutorId = tutor.UserId,
                    CreatedDate = DateTime.Now,
                    UserId = user.UserId,
                    Course = course,
                    Status = Status.Ожидает,
                    StartDate = dateTime
                };

                //  var waited = TestData.Schedules.FirstOrDefault(m => m.UserId == Convert.ToInt32(user_id) && m.TutorId == Convert.ToInt32(tutor_id) && m.WaitPaymentDate != DateTime.MinValue);
                //if (waited != null)
                //{
                //    if (dateTime < waited.WaitPaymentDate)
                //    {
                //        waited.WaitPaymentDate = DateTime.MinValue;


                //        if (user.LessonsCount == 0)
                //        {


                //            //if (sch.Looped)
                //            //{
                //            //    if (sch.ReadyDates.Count > 0)
                //            //    {
                //            //        sch.WaitPaymentDate = sch.ReadyDates.Last().AddDays(7);
                //            //    }
                //            //    else
                //            //    {
                //            //        if (sch.RescheduledLessons.Count > 0)
                //            //        {
                //            //            sch.WaitPaymentDate = sch.RescheduledLessons.Last().NewTime;
                //            //        }
                //            //        else if (sch.RescheduledDate != DateTime.MinValue)
                //            //        {
                //            //            sch.WaitPaymentDate = sch.RescheduledDate;
                //            //        }
                //            //        else
                //            //        {
                //            //            sch.WaitPaymentDate = sch.StartDate;
                //            //        }

                //            //    }
                //            //}
                //            //else
                //            //{
                //            //    sch.WaitPaymentDate = sch.StartDate;
                //            //}
                //        }
                //    }
                //}

                TestData.Schedules.Add(sch);

                var list = TestData.Schedules.Where(m => m.UserId == Convert.ToInt32(user_id) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue).Reverse().ToList();
                foreach (var item in list)
                {
                    if (item.WaitPaymentDate != DateTime.MinValue)
                    {
                        item.WaitPaymentDate = DateTime.MinValue;
                    }
                }
                var sorted = ScheduleService.SortSchedulesForUnpaid(list);


                foreach (var item in sorted)
                {

                    var sch2 = TestData.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);

                    sch2.WaitPaymentDate = item.Nearest;
                }


                // отправка что новый урок у репетитора
                var type = Convert.ToBoolean(split[2]) == true ? "постоянное" : "разовое";

                NotifHub.SendNotification(Constants.NOTIF_NEW_LESSON.Replace("{studentName}", user.FirstName + " " + user.LastName).Replace("{type}", type)
                    .Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateTime.ToString("dd.MM.yyyy HH:mm")), user_id.ToString(), _hubContext);


                NotifHub.SendNotification(Constants.NOTIF_NEW_LESSON.Replace("{studentName}", user.FirstName + " " + user.LastName).Replace("{type}", type)
                  .Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateTime.ToString("dd.MM.yyyy HH:mm")), TestData.Managers.First().UserId.ToString(), _hubContext);
            }

            return tutor;
        }

        public bool RejectStudent(string[] args, IHubContext<NotifHub> _hubContext)
        {
            var tutorId = Convert.ToInt32(args[0]);
            var userId = Convert.ToInt32(args[1]);
            var tutorName = TestData.Tutors.FirstOrDefault(m => m.UserId == tutorId);
            var userName = TestData.UserList.FirstOrDefault(m => m.UserId == userId);

            while (Program.BackInAir == true)
            {
                Thread.Sleep(10);
            }
            var all_schedules = TestData.Schedules.Where(m => m.UserId == userId && m.TutorId == tutorId).ToList();
            foreach (var item in all_schedules)
            {
                TestData.Schedules.Remove(item);
            }

            var all_reschedules = TestData.RescheduledLessons.Where(m => m.UserId == userId && m.TutorId == tutorId);
            foreach (var item in all_reschedules)
            {
                TestData.RescheduledLessons.Remove(item);
            }



            var list = TestData.Schedules.Where(m => m.UserId == userId && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue).Reverse().ToList();
            foreach (var item in list)
            {
                if (item.WaitPaymentDate != DateTime.MinValue)
                {
                    item.WaitPaymentDate = DateTime.MinValue;
                }
            }
            var sorted = ScheduleService.SortSchedulesForUnpaid(list);


            foreach (var item in sorted)
            {

                var sch2 = TestData.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);

                sch2.WaitPaymentDate = item.Nearest;
            }


            var managerId = TestData.Managers.First().UserId;



            NotifHub.SendNotification(Constants.NOTIF_TUTOR_REJECT_USER_FMANAGER
                .Replace("{tutorName}", tutorName.FirstName + " " + tutorName.LastName)
                .Replace("{userName}", userName.FirstName + " " + userName.LastName), managerId.ToString(), _hubContext);


            NotifHub.SendNotification(Constants.NOTIF_TUTOR_REJECT_USER_FUSER.Replace("{tutorName}", tutorName.FirstName + " " + tutorName.LastName), userId.ToString(), _hubContext);


            return true;
        }

        public bool RemoveTutor(string args)
        {
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(args);
            if (user != null)
            {
                TestData.Tutors.RemoveAll(m => m.UserId == user.UserId);
                if (TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId) != null)
                {
                    ChatHub.RemoveContact(user.UserId);
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


                var list = TestData.Schedules.Where(m => m.UserId == Convert.ToInt32(user_id) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue).Reverse().ToList();
                foreach (var item in list)
                {
                    if (item.WaitPaymentDate != DateTime.MinValue)
                    {
                        item.WaitPaymentDate = DateTime.MinValue;
                    }
                }
                var sorted = ScheduleService.SortSchedulesForUnpaid(list);


                foreach (var item in sorted)
                {

                    var sch2 = TestData.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);

                    sch2.WaitPaymentDate = item.Nearest;
                }


                string type = schedule.Looped == true ? "постоянное" : "разовое";

                NotifHub.SendNotification(Constants.NOTIF_REMOVE_LESSON.Replace("{tutorName}", schedule.TutorFullName).Replace("{type}", type).Replace("{studentName}", schedule.UserName).Replace("{date}", dateTime.ToString("dd.MM.yyyy HH:mm")), user_id, _hubContext);

                NotifHub.SendNotification(Constants.NOTIF_REMOVE_LESSON.Replace("{tutorName}", schedule.TutorFullName).Replace("{type}", type).Replace("{studentName}", schedule.UserName).Replace("{date}", dateTime.ToString("dd.MM.yyyy HH:mm")), manager_id.ToString(), _hubContext);
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


            var schedules = TestData.Schedules.Where(m => m.TutorId == tutor.UserId).ToList();

            foreach (var item in schedules)
            {
                var new_name = "";
                var name = item.TutorFullName.Split(" ");
                if (name[0] != tutor.FirstName)
                {
                    new_name = tutor.FirstName;
                }
                else
                {
                    new_name = name[0];
                }
                new_name += " ";
                if (name[1] != tutor.LastName)
                {
                    new_name += tutor.LastName;
                }
                else
                {
                    new_name += name[1];
                }

                item.TutorFullName = new_name;
            }


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

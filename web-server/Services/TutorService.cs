using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class TutorService : ITutorService
    {
        private readonly UserRepository _userRepository;
        private readonly ScheduleRepository _scheduleRepository;
        private readonly CourseRepository _courseRepository;
        private readonly NotificationRepository _notificationRepository;

        public TutorService(UserRepository userRepository, ScheduleRepository scheduleRepository, CourseRepository courseRepository)
        {
            _courseRepository= courseRepository;
            _userRepository = userRepository;
            _scheduleRepository = scheduleRepository;
        }

        public async Task<Tutor> AddTutor(string args)
        {
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Tutor>(args.ToString());

            await _userRepository.AddUser(model);

            return model;
        }
        public async Task<Tutor> GetTutor(string args)
        {
            return (Tutor)await _userRepository.GetUserById(Convert.ToInt32(args));
        }
        public async Task<List<Tutor>> GetAll()
        {
            return await _userRepository.GetAllTutors();
        }
        public async Task<Tutor> AddTutorFreeDate(string args)
        {
            var split = args.Split(';');
            var tutor_id = split[0];
            var dateTime = DateTime.Parse(split[1]);
            var tutor = (Tutor) await _userRepository.GetUserById(Convert.ToInt32(tutor_id));
            // var tutor =(Tutor) await _userRepository.GetUserById(Convert.ToInt32(tutor_id));
            if (tutor != null)
            {
                await _userRepository.SetTutorFreeDate(tutor.UserId, dateTime);
                //TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id)).UserDates.dateTimes.Add(dateTime);
                var model = new Schedule()
                {
                    Date = new UserDate() { dateTimes = new List<DateTime>() { dateTime } },
                    Looped = Convert.ToBoolean(split[2]),
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    TutorId = tutor.UserId,
                    UserId = -1,
                    StartDate = dateTime,
                };
                await _scheduleRepository.AddSchedule(model);
                //TestData.Schedules.Add();

            }

            return tutor;
        }

        public async Task<Tutor> AddTutorSchedule(string args, IHubContext<NotifHub> _hubContext)
        {
            var split = args.Split(';');
            var tutor_id = split[0];
            var user_id = split[3];
            var course_id = split[4];

            var dateTime = DateTime.Parse(split[1]);
            var tutor = (Tutor) await _userRepository.GetUserById(Convert.ToInt32(tutor_id));
            var user = (Student)await _userRepository.GetUserById(Convert.ToInt32(user_id));
            var course = await _courseRepository.GetCourseById(Convert.ToInt32(course_id));
            //var tutor =(Tutor) await _userRepository.GetUserById(Convert.ToInt32(tutor_id));
            //var user = TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(user_id));
           // var course = TestData.Courses.FirstOrDefault(m => m.Id == Convert.ToInt32(course_id));
            if (user.Credit.Where(m => m.Repaid == false).ToList().Count >= 3)
            {
                return null;
            }

            if (tutor != null)
            {
                var sch = new Schedule()
                {
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


                await _scheduleRepository.AddSchedule(sch);
                //TestData.Schedules.Add(sch);

                var list = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == Convert.ToInt32(user_id) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue);
                list.Reverse();
                
              //  var list = TestData.Schedules.Where(m => m.UserId == Convert.ToInt32(user_id) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue).Reverse().ToList();
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

                    var sch2 = await _scheduleRepository.GetScheduleById(item.ScheduleId);
                    //var sch2 = TestData.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);

                    sch2.WaitPaymentDate = item.Nearest;
                }


                // отправка что новый урок у репетитора
                var type = Convert.ToBoolean(split[2]) == true ? "постоянное" : "разовое";

                NotifHub.SendNotification(Constants.NOTIF_NEW_LESSON.Replace("{studentName}", user.FirstName + " " + user.LastName).Replace("{type}", type)
                    .Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateTime.ToString("dd.MM.yyyy HH:mm")), user_id.ToString(), _hubContext, _userRepository, _notificationRepository );


                NotifHub.SendNotification(Constants.NOTIF_NEW_LESSON.Replace("{studentName}", user.FirstName + " " + user.LastName).Replace("{type}", type)
                  .Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateTime.ToString("dd.MM.yyyy HH:mm")), (await _userRepository.GetManagerId()).ToString(), _hubContext, _userRepository, _notificationRepository);
            }

            return tutor;
        }

        public async Task<bool> RejectStudent(string[] args, IHubContext<NotifHub> _hubContext)
        {
            var tutorId = Convert.ToInt32(args[0]);
            var userId = Convert.ToInt32(args[1]);
            var tutorName = await _userRepository.GetUserById(tutorId);
            //TestData.UserList.FirstOrDefault(m => m.UserId == tutorId);
            //var userName = TestData.UserList.FirstOrDefault(m => m.UserId == userId);
            var userName = await _userRepository.GetUserById(userId);
            while (Program.BackInAir == true)
            {
                Thread.Sleep(10);
            }

            var all_schedules = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == userId && m.TutorId == tutorId);
            //var all_schedules = TestData.Schedules.Where(m => m.UserId == userId && m.TutorId == tutorId).ToList();
            foreach (var item in all_schedules)
            {
                await _scheduleRepository.RemoveSchedule(item);
                //TestData.Schedules.Remove(item);
            }

            var all_reschedules = await _scheduleRepository.GetReschedulesByFunc(m => m.UserId == userId && m.TutorId == tutorId); 
            //var all_reschedules = TestData.RescheduledLessons.Where(m => m.UserId == userId && m.TutorId == tutorId);
            foreach (var item in all_reschedules)
            {
                await _scheduleRepository.RemoveReschedule(item);
                //TestData.RescheduledLessons.Remove(item);
            }


            var list = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == userId && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue);
            list.Reverse();
            //var list = TestData.Schedules.Where(m => m.UserId == userId && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue).Reverse().ToList();
            foreach (var item in list)
            {
                if (item.WaitPaymentDate != DateTime.MinValue)
                {
                    item.WaitPaymentDate = DateTime.MinValue;
                   // await _scheduleRepository.Update(item);

                }
            }
            await _scheduleRepository.UpdateRange(list);

            var sorted = ScheduleService.SortSchedulesForUnpaid(list);


            foreach (var item in sorted)
            {

                var sch2 = await _scheduleRepository.GetScheduleById(item.ScheduleId);
                // TestData.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);

                sch2.WaitPaymentDate = item.Nearest;

                await _scheduleRepository.Update(sch2);
            }

            //await _scheduleRepository.UpdateRange(sorted);

            var managerId = _userRepository.GetManagerId();
            // TestData.UserList.FirstOrDefault(m => m.Role == "Manager").UserId;
            


            NotifHub.SendNotification(Constants.NOTIF_TUTOR_REJECT_USER_FMANAGER
                .Replace("{tutorName}", tutorName.FirstName + " " + tutorName.LastName)
                .Replace("{userName}", userName.FirstName + " " + userName.LastName), managerId.ToString(), _hubContext, _userRepository, _notificationRepository);


            NotifHub.SendNotification(Constants.NOTIF_TUTOR_REJECT_USER_FUSER.Replace("{tutorName}", tutorName.FirstName + " " + tutorName.LastName), userId.ToString(), _hubContext, _userRepository, _notificationRepository);


            return true;
        }

        public async Task<bool> RemoveTutor(string args)
        {
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<Tutor>(args);
            if (user != null)
            {
                await _userRepository.Remove(user.UserId);
                //TestData.UserList.RemoveAll(m => m.UserId == user.UserId);
                //if (TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId) != null)
                //{
                //    ChatHub.RemoveContact(user.UserId);
                //    TestData.UserList.RemoveAll(m => m.UserId == user.UserId);
                //    return true;
                //}
            }

            return true;
        }

        public async Task<Tutor> RemoveTutorSchedule(string args, IHubContext<NotifHub> _hubContext)
        {

            var split = args.Split(';');
            var dateTime = DateTime.Parse(split[2]);
            var tutor_id = split[0];
            var user_id = split[1];
            var curr = DateTime.Parse(split[3]);
            var manager_id = await _userRepository.GetManagerId();
            var tutor =(Tutor) await _userRepository.GetUserById(Convert.ToInt32(tutor_id));
            //var tutor =(Tutor) await _userRepository.GetUserById(Convert.ToInt32(tutor_id));
            if (tutor != null)
            {
                tutor.UserDates.dateTimes.Remove(dateTime);
                await _userRepository.Update(tutor);
                Schedule schedule = await _scheduleRepository.GetScheduleByFunc(m => m.TutorId == Convert.ToInt32(tutor_id) && m.Date.dateTimes[0] == dateTime && m.UserId == Convert.ToInt32(user_id));
                //var schedule = TestData.Schedules.FirstOrDefault();
                if (schedule != null)
                {
                    schedule.RemoveDate = curr;
                }
                await _scheduleRepository.Update(schedule);

                var list = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == Convert.ToInt32(user_id) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue);
                list.Reverse();
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

                    var sch2 = await _scheduleRepository.GetScheduleById(item.ScheduleId);
                    //var sch2 = TestData.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);

                    sch2.WaitPaymentDate = item.Nearest;

                    await _scheduleRepository.Update(sch2);
                }


                string type = schedule.Looped == true ? "постоянное" : "разовое";

                NotifHub.SendNotification(Constants.NOTIF_REMOVE_LESSON.Replace("{tutorName}", schedule.TutorFullName).Replace("{type}", type).Replace("{studentName}", schedule.UserName).Replace("{date}", dateTime.ToString("dd.MM.yyyy HH:mm")), user_id, _hubContext, _userRepository, _notificationRepository);

                NotifHub.SendNotification(Constants.NOTIF_REMOVE_LESSON.Replace("{tutorName}", schedule.TutorFullName).Replace("{type}", type).Replace("{studentName}", schedule.UserName).Replace("{date}", dateTime.ToString("dd.MM.yyyy HH:mm")), manager_id.ToString(), _hubContext, _userRepository, _notificationRepository);
            }

            return tutor;
        }

        public async Task<Tutor> RemoveTutorTime(string args)
        {
            var split = args.Split(';');
            var tutor_id = split[0];
            var dateTime = DateTime.Parse(split[1]);
            var tutor =(Tutor) await _userRepository.GetUserById(Convert.ToInt32(tutor_id));
            if (tutor != null)
            {
                tutor.UserDates.dateTimes.Remove(dateTime);
                await _userRepository.Update(tutor);
                //TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id)).UserDates.dateTimes.Remove(dateTime);
            }

            return tutor;
        }

        public async Task<Tutor> UpdateTutor(string args)
        {
            var tutor = Newtonsoft.Json.JsonConvert.DeserializeObject<Tutor>(args);
            var old = await _userRepository.GetUserById(tutor.UserId);
            // var old = TestData.UserList.FirstOrDefault(m => m.UserId == tutor.UserId);

            var schedules = await _scheduleRepository.GetSchedulesByFunc(m => m.TutorId == tutor.UserId);
          //  var schedules = TestData.Schedules.Where(m => m.TutorId == tutor.UserId).ToList();

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

            await _userRepository.Update(tutor);

            return tutor;
        }

    }
}

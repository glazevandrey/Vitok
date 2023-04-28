using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class TutorService : ITutorService
    {
        private readonly UserRepository _userRepository;
        private readonly ScheduleRepository _scheduleRepository;
        private readonly CourseRepository _courseRepository;
        private readonly NotificationRepository _notificationRepository;
        IMapper _mapper;

        public TutorService(IMapper mapper, UserRepository userRepository, ScheduleRepository scheduleRepository, CourseRepository courseRepository)
        {
            _mapper = mapper;
            _courseRepository = courseRepository;
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
            var tutor = await _userRepository.GetTutor(Convert.ToInt32(tutor_id));
            if (tutor != null)
            {
                tutor.UserDates.Add(new UserDate() { dateTime = dateTime });
                var model = new Schedule()
                {
                    Looped = Convert.ToBoolean(split[2]),
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    TutorId = tutor.UserId,
                    UserId = 1,
                    Course = new Course() { Id = 1 },
                    StartDate = dateTime,
                };

                await _scheduleRepository.AddSchedule(model);
                //tutor.Schedules.Add(_mapper.Map<ScheduleDTO>(model));
                await _userRepository.SaveChanges(tutor);



                //await _scheduleRepository.AddSchedule(model);
            }



            return _mapper.Map<Tutor>(tutor);
        }

        public async Task<Tutor> AddTutorSchedule(string args, IHubContext<NotifHub> _hubContext)
        {
            var split = args.Split(';');
            var tutor_id = split[0];
            var user_id = split[3];
            var course_id = Convert.ToInt32(split[4]);

            var dateTime = DateTime.Parse(split[1]);
            var tutor = (Tutor)await _userRepository.GetLiteUser(Convert.ToInt32(tutor_id));
            var user = await _userRepository.GetStudent(Convert.ToInt32(user_id));
            //var user = (Student)await _userRepository.GetUserById(Convert.ToInt32(user_id)); //await _userRepository.GetStudent(Convert.ToInt32(course_id));
           // var course = await _courseRepository.GetCourseById(Convert.ToInt32(course_id));

            if (user.Credit.Where(m => m.Repaid == false).ToList().Count >= 3)
            {
                return null;
            }

            if (tutor != null)
            {
                
                var sch = new ScheduleDTO()
                {
                    UserName = user.FirstName + " " + user.LastName,
                    Looped = Convert.ToBoolean(split[2]),
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    TutorId = tutor.UserId,
                    CreatedDate = DateTime.Now,
                    UserId = user.UserId,
                    //CourseId = Convert.ToInt32(course_id),
                    CourseId = course_id,
                    Status = Status.Ожидает,
                    StartDate = dateTime
                };

                //await _scheduleRepository.AddSchedule(sch);
                user.Schedules.Add(sch);

                await _userRepository.SaveChanges(user);

                var list = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == Convert.ToInt32(user_id) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue);

                var sorted = ScheduleService.SortSchedulesForUnpaid(list);
                foreach (var item in sorted)
                {
                    foreach (var sch2 in list)
                    {
                        if(item.ScheduleId == sch2.Id)
                        {
                            sch2.WaitPaymentDate = item.Nearest;

                        }
                        else
                        {
                            sch2.WaitPaymentDate = DateTime.MinValue;
                        }

                        await _scheduleRepository.Update(sch2);
                    }
                }

                
                var type = Convert.ToBoolean(split[2]) == true ? "постоянное" : "разовое";
                Task.Run(async () =>
                {
                    await NotifHub.SendNotification(Constants.NOTIF_NEW_LESSON.Replace("{studentName}", user.FirstName + " " + user.LastName).Replace("{type}", type)
                    .Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}",
                    dateTime.ToString("dd.MM.yyyy HH:mm")), user_id.ToString(), _hubContext, _mapper).ConfigureAwait(false);

                    await NotifHub.SendNotification(Constants.NOTIF_NEW_LESSON.Replace("{studentName}", user.FirstName + " " + user.LastName).Replace("{type}", type)
                      .Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}",
                      dateTime.ToString("dd.MM.yyyy HH:mm")), (await _userRepository.GetManagerId()).ToString(), _hubContext, _mapper).ConfigureAwait(false);

                });
            }

            return tutor;
        }

        public async Task<bool> RejectStudent(string[] args, IHubContext<NotifHub> _hubContext)
        {
            var tutorId = Convert.ToInt32(args[0]);
            var userId = Convert.ToInt32(args[1]);
            var tutorName = await _userRepository.GetLiteUser(tutorId);
            //TestData.UserList.FirstOrDefault(m => m.UserId == tutorId);
            //var userName = TestData.UserList.FirstOrDefault(m => m.UserId == userId);
            var userName = await _userRepository.GetLiteUser(userId);
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

            var managerId = await _userRepository.GetManagerId();
            Task.Run(async () =>
            {
                await NotifHub.SendNotification(Constants.NOTIF_TUTOR_REJECT_USER_FMANAGER
                .Replace("{tutorName}", tutorName.FirstName + " " + tutorName.LastName)
                .Replace("{userName}", userName.FirstName + " " + userName.LastName), managerId.ToString(), _hubContext, _mapper);


                await NotifHub.SendNotification(Constants.NOTIF_TUTOR_REJECT_USER_FUSER.Replace("{tutorName}", tutorName.FirstName + " " + tutorName.LastName), userId.ToString(), _hubContext, _mapper);
            });

            return true;
        }

        public async Task<bool> RemoveTutor(string args)
        {
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<Tutor>(args);
            if (user != null)
            {
                await _userRepository.Remove(user.UserId);

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

            if (tutor_id != null)
            {

                ScheduleDTO schedule = await _scheduleRepository.GetScheduleByFunc(m => m.TutorId == Convert.ToInt32(tutor_id) && m.StartDate == dateTime && m.UserId == Convert.ToInt32(user_id) && m.RemoveDate == DateTime.MinValue);

                if (schedule != null)
                {
                    schedule.RemoveDate = curr;
                }

                await _scheduleRepository.Update(schedule);

                if (user_id == "1")
                {
                    var tutor = await _userRepository.GetTutor(Convert.ToInt32(tutor_id));
                    var date = tutor.UserDates.FirstOrDefault(m => m.dateTime == dateTime);
                    tutor.UserDates.Remove(date);
                    await _userRepository.SaveChanges(tutor);
                }

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

                    sch2.WaitPaymentDate = item.Nearest;

                    await _scheduleRepository.Update(sch2);
                }


                string type = schedule.Looped == true ? "постоянное" : "разовое";
                Task.Run(async () =>
                {
                    await NotifHub.SendNotification(Constants.NOTIF_REMOVE_LESSON.Replace("{tutorName}", schedule.TutorFullName).Replace("{type}", type).Replace("{studentName}", schedule.UserName).Replace("{date}", dateTime.ToString("dd.MM.yyyy HH:mm")), user_id, _hubContext, _mapper);

                    await NotifHub.SendNotification(Constants.NOTIF_REMOVE_LESSON.Replace("{tutorName}", schedule.TutorFullName).Replace("{type}", type).Replace("{studentName}", schedule.UserName).Replace("{date}", dateTime.ToString("dd.MM.yyyy HH:mm")), manager_id.ToString(), _hubContext, _mapper);

                });
            }
            return null;
        }

        public async Task<Tutor> UpdateTutor(string args)
        {

            var tutor = Newtonsoft.Json.JsonConvert.DeserializeObject<Tutor>(args);
      
            var old = await _userRepository.GetTutor(tutor.UserId);
            var schedules = old.Schedules;
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

            old.Password = tutor.Password;
            old.Phone = tutor.Phone;

            var mapped = _mapper.Map<List<TutorCourse>>(tutor.Courses);
            foreach (var item in mapped)
            {
                item.Id = 0;
                if(item.TutorId == null)
                {
                    item.TutorId = old.UserId;
                }
            }

            old.Courses = mapped;
            

            await _userRepository.SaveChanges(old);

            return tutor;
        }

    }
}

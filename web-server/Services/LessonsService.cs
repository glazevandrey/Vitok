using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Database;
using web_server.Database.Repositories;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class LessonsService : ILessonsService
    {
        UserRepository _userRepository;
        ScheduleRepository _scheduleRepository;
        CourseRepository _courseRepository;
        TariffRepositories _tariffRepositories;
        NotificationRepository _notificationRepository;
        IMapper _mapper;
        public LessonsService(IMapper mapper, UserRepository userRepository,  ScheduleRepository scheduleRepository, TariffRepositories tariffRepositories, 
            CourseRepository courseRepository, NotificationRepository notificationRepository)
        {
            _mapper = mapper;
            _scheduleRepository = scheduleRepository;
            _userRepository = userRepository;
            _tariffRepositories = tariffRepositories;
            _courseRepository = courseRepository;
            _notificationRepository = notificationRepository;
        }

        //public async Task<List<RescheduledLessons>> GetRescheduledLessons(string args)
        //{
        //    var user = await _userRepository.GetUserByToken(args);
        //  //  var user = TestData.UserList.FirstOrDefault(m => m.ActiveToken == args);
        //    if (user == null)
        //    {
        //        return null;
        //    }

        //    var schedules = new List<RescheduledLessons>();
        //    if (user.Role == "Tutor")
        //    {
        //        schedules = await _scheduleRepository.GetReschedulesByFunc(m => m.TutorId == user.UserId);
        //        //schedules = TestData.RescheduledLessons.Where(m => m.TutorId == user.UserId).ToList();
        //    }
        //    else
        //    {
        //        schedules = await _scheduleRepository.GetReschedulesByFunc(m => m.UserId == user.UserId);
        //    }

        //    //if (schedules == null || schedules.Count == 0)
        //    //{
        //    //    schedules = TestData.RescheduledLessons.Where(m => m.TutorId == user.UserId).ToList();
        //    //}

        //    return schedules;
        //}



        public async Task<User> AddLessonsToUser(string[] args)
        {
            var user = await _userRepository.GetStudent(Convert.ToInt32(args[0]));
            //var user = (Student)(await _userRepository.GetUserById(Convert.ToInt32(args[0])));
           // var user = (Student)TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(args[0]));

            user.LessonsCount += Convert.ToInt32(args[1]);
            //var schedules = user.Schedules;//_scheduleRepository.GetSchedulesByFunc(m => m.UserId == user.UserId);

            //var schedules = TestData.Schedules.Where(m => m.UserId == user.UserId).ToList();
            var isTrial = Convert.ToBoolean(args[2]);
            if (isTrial)
            {
                user.UsedTrial = true;
            }

            var lessonCount = Convert.ToInt32(args[1]);
            var tariff = await _tariffRepositories.GetTariffByLessonsCount(lessonCount);
            //var tariff = TestData.Tariffs.FirstOrDefault(m => m.LessonsCount == lessonCount);
            if (user.WasFirstPayment == false)
            {
                user.WasFirstPayment = true;
            }

            if (tariff != null)
            {
                var one = tariff.Amount / tariff.LessonsCount;

                var needed = user.Money.FirstOrDefault(m => m.Cost == one);
                if (needed == null)
                {

                    user.Money.Add(new UserMoney() { Cost = one, Count = lessonCount });
                }
                else
                {
                    user.Money.FirstOrDefault(m => m.Cost == one).Count += lessonCount;
                }

                user.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(tariff.Amount), Count = lessonCount }, CustomMessage = $"Оплата тарифа: {tariff.Title}" });

                //TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).BalanceHistory.Add(new BalanceHistory()
                //{ CashFlow = new CashFlow() { Amount = (int)Math.Abs(tariff.Amount), Count = lessonCount} ,  CustomMessage = $"Оплата тарифа: {tariff.Title}"  });

            }
            else
            {
                if (isTrial)
                {
                    user.BalanceHistory.Add(new BalanceHistory() { Date = DateTime.Now.AddDays(2), CashFlow = new CashFlow() { Amount = 250, Count = 1 }, CustomMessage = $"Оплачено пробное занятие" });

                    //TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).BalanceHistory.Add(new BalanceHistory() { Date = DateTime.Now.AddDays(2), CashFlow = new CashFlow() { Amount = 250, Count = 1 }, CustomMessage = $"Оплачено пробное занятие" });
                }
                else
                {
                    user.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = 1000 * lessonCount, Count = lessonCount }, CustomMessage = $"Оплачено занятий: {lessonCount}" });
                }

                var id = user.Money.FirstOrDefault() != null ? user.Money.FirstOrDefault().Id : 0;

                if(user.Money.FirstOrDefault(m=>m.Cost == 1000) != null)
                {
                    user.Money.FirstOrDefault(m=>m.Cost == 1000).Count+= lessonCount;

                }
                else
                {
                    user.Money.Add(new UserMoney() { Id = id, Cost = 1000, Count = lessonCount });
                }
            }


            var manager = await _userRepository.GetUser(await _userRepository.GetManagerId());

            int how_minus = 0;
            if (user.Credit.Where(m => m.Repaid == false).ToList().Count != 0)
            {
                foreach (var item in user.Money)
                {

                    if (item.Count == 0)
                    {
                        continue;
                    }

                    for (int i = 0; i < item.Count; i++)
                    {
                        var count = user.Credit.Where(m => m.Repaid == false).ToList().Count;
                        if (count == 0)
                        {
                            break;
                        }

                        var tutor = await _userRepository.GetTutor(user.Credit.First().TutorId);
                       // var tutor = TestData.UserList.FirstOrDefault(m => m.UserId == user.Credit.First().TutorId);

                        var f_tut = Math.Abs(item.Cost / 100 * 60);
                        var f_manag = Math.Abs(item.Cost / 100 * 40);

                        tutor.Balance += f_tut;
                        tutor.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(f_tut) }, CustomMessage = $"Оплата долга за 1 занятие. Студент: {user.FirstName} {user.LastName}" });



                        manager.Balance += f_manag;
                        manager.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(f_manag) }, CustomMessage = $"Оплата долга за 1 занятие. Студент: {user.FirstName} {user.LastName}. Репетитор: {tutor.FirstName} {tutor.LastName}" });

                        how_minus++;
                        user.BalanceHistory.Add(new BalanceHistory() { CustomMessage = $"Погашен долг за 1 занятие с репетитором {tutor.FirstName} {tutor.LastName}" });

                        var credit = user.Credit.Where(m => m.Repaid == false).First();
                        credit.Repaid = true;
                        credit.Amount = item.Cost;

                        await _userRepository.SaveChanges(tutor);
                        await _userRepository.SaveChanges(manager);
                        //await _userRepository.Update(tutor);
                        //await _userRepository.Update(manager);
                    }

                    item.Count -= how_minus;
                }
            }
        
           // var waited = schedules.Where(m => m.WaitPaymentDate != DateTime.MinValue).ToList();
           

           

            var waited = user.Schedules.Where(m => m.WaitPaymentDate != DateTime.MinValue && m.UserId == user.UserId).ToList();
            if (waited.Count != 0 && user.LessonsCount > 0)
            {
                foreach (var item in waited)
                {
                    item.WaitPaymentDate = DateTime.MinValue;



                    if (user.Credit.Where(m => m.Repaid == false).ToList().Count == 0)
                    {
                        user.StartWaitPayment = DateTime.MinValue;
                    }
                }

                await _userRepository.SaveChanges(user);

            }
            else
            {
                await _userRepository.SaveChanges(user);

            }
            return  _mapper.Map<Student>(user);
        }

        public async Task<Schedule> RescheduleLesson(string args, IHubContext<NotifHub> _hubContext)
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
            var tutor = await _userRepository.GetTutor(tutor_id);
            var user = await _userRepository.GetStudent(user_id);


            if (Convert.ToBoolean(loop))
            {
                var alredyUsed = tutor.Schedules.Where(m=>m.StartDate.DayOfWeek == newDateTime.DayOfWeek && m.StartDate.Hour == newDateTime.Hour).ToList();
                //var alredyUsed = await _scheduleRepository.GetSchedulesByFunc(m => m.TutorId == tutor_id && );
                if (alredyUsed.Count != 0)
                {
                    return null;
                }
                var new_model = new Schedule
                {
                    TutorId = tutor_id,
                    UserId = user_id,
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    UserName = user.FirstName + " " + user.LastName,
                    Course = await _courseRepository.GetCourseById(courseId),
                    StartDate = newDateTime,
                    Looped = true,
                };

                var model = await _scheduleRepository.GetScheduleByFunc(m => m.TutorId == tutor_id && m.UserId == user_id && m.StartDate == oldDateTime); //tutor.Schedules.FirstOrDefault(m=>m.UserId == user_id && m.StartDate == oldDateTime);
                //var model = await _scheduleRepository.GetScheduleByFunc(m => m.TutorId == tutor_id && m.UserId == user_id && m.StartDate == oldDateTime);

                if (model.WaitPaymentDate != DateTime.MinValue)
                {
                    new_model.WaitPaymentDate = model.WaitPaymentDate;
                }

                model.Status = Status.Перенесен;
                model.RescheduledDate = cureDate;
                model.NewDate = newDateTime;

                await _scheduleRepository.Update(model);
                await _scheduleRepository.AddSchedule(new_model);

                // отправка манагеру что постоянный перенос
                await NotifHub.SendNotification(Constants.NOTIF_REGULAR_RESCHEDULE.Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName)
                    .Replace("{studentName}", user.FirstName + " " + user.LastName)
                    .Replace("{oldDate}", cureDate.ToString("dd.MM.yyyy HH:mm"))
                    .Replace("{newDate}", newDateTime.ToString("dd.MM.yyyy HH:mm")),(await _userRepository.GetManagerId()).ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);


                // отправка студенту что перенос занятия
                await NotifHub.SendNotification(Constants.NOTIF_LESSON_WAS_RESCHEDULED_FOR_STUDENT_REGULAR
                    .Replace("{name}", tutor.FirstName + " " + tutor.LastName)
                    .Replace("{dateOld}", cureDate.ToString("dd.MM.yyyy HH:mm"))
                    .Replace("{dateNew}", newDateTime.ToString("dd.MM.yyyy HH:mm")), user_id.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);

                await CalculateNoPaidWarn(user, _hubContext);

                return new_model;
            }
            else
            {
                var alredyUsed = tutor.Schedules.Where(m => m.StartDate == newDateTime).ToList();
                //var alredyUsed = await _scheduleRepository.GetSchedulesByFunc(m => m.TutorId == tutor_id && m.StartDate == newDateTime);
                //var alredyUsed = TestData.Schedules.Where(m => m.TutorId == tutor_id && m.StartDate == newDateTime).ToList();
                if (alredyUsed.Count != 0)
                {
                    return null;
                }

                var model = new RescheduledLessons
                {
                    Initiator = initiator,
                    NewTime = newDateTime,
                    OldTime = oldDateTime,
                    Reason = reason,          
                };

                var new_model = new Schedule
                {
                    TutorId = tutor_id,
                    UserId = user_id,
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    UserName = user.FirstName + " " + user.LastName,
                    Course = await _courseRepository.GetCourseById(courseId),
                    StartDate = newDateTime,
                    Looped = false,
                };

                var re_less = new RescheduledLessons() { Initiator = initiator, NewTime = newDateTime, OldTime = cureDate, Reason = reason };

                var old = await _scheduleRepository.GetScheduleByFunc(m => m.TutorId == tutor_id && m.UserId == user_id && m.StartDate == oldDateTime && m.RemoveDate == DateTime.MinValue);
                //var old = tutor.Schedules.FirstOrDefault(m=>m.UserId == user_id && m.StartDate == oldDateTime);
                //var old = await _scheduleRepository.GetScheduleByFunc(m => m.TutorId == tutor_id && m.UserId == user_id && m.StartDate == oldDateTime);
                //var old = TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime);
                if (old.Status == Status.ОжидаетОплату)
                {
                    new_model.Status = Status.ОжидаетОплату;
                }
                else
                {
                    old.Status = Status.Перенесен;

                }

                old.RescheduledLessons.Add(re_less);
                await _scheduleRepository.Update(old);
                //await _scheduleRepository.Update(_mapper.Map<ScheduleDTO>(old));
                //await _userRepository.SaveChanges(tutor);
                await _scheduleRepository.AddSchedule(new_model);
                //TestData.Schedules.Add(new_model);

                await NotifHub.SendNotification(Constants.NOTIF_LESSON_WAS_RESCHEDULED_FOR_STUDENT
                   .Replace("{name}", tutor.FirstName + " " + tutor.LastName)
                   .Replace("{dateOld}", cureDate.ToString("dd.MM.yyyy HH:mm"))
                   .Replace("{dateNew}", newDateTime.ToString("dd.MM.yyyy HH:mm")), user_id.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);

                // отправка манагеру что разовый перенос
                await NotifHub.SendNotification(Constants.NOTIF_RESCHEDULE.Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName)
                    .Replace("{studentName}", user.FirstName + " " + user.LastName)
                    .Replace("{oldDate}", cureDate.ToString("dd.MM.yyyy HH:mm"))
                    .Replace("{newDate}", newDateTime.ToString("dd.MM.yyyy HH:mm")), (await _userRepository.GetManagerId()).ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);



                if (user.LessonsCount <= 0)
                {
                    if (user.StartWaitPayment == DateTime.MinValue || user.StartWaitPayment == null)
                    {
                        user.StartWaitPayment = DateTime.Now;
                    }
                    //var ff = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == user_id && m.WaitPaymentDate != DateTime.MinValue);
                    //// var ff = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == user_id && m.WaitPaymentDate != DateTime.MinValue);
                    //if (ff.Count > 0)
                    //{
                    //    foreach (var item in ff)
                    //    {
                    //        item.WaitPaymentDate = DateTime.MinValue;
                    //    }
                    //}

                    var managerId = await _userRepository.GetManagerId();
                    await NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT, user_id.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);
                    await NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT_FOR_MANAGER.Replace("{name}",
                        user.FirstName + " " + user.LastName),managerId.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);
                }

                await CalculateNoPaidWarn(user, _hubContext);
                return new_model;
            }
        }
        public async Task CalculateNoPaidWarn(StudentDTO user, IHubContext<NotifHub> _hubContext)
        {

            if (user.LessonsCount <= 0)
            {
                if (user.StartWaitPayment == DateTime.MinValue || user.StartWaitPayment == null)
                {
                    user.StartWaitPayment = DateTime.Now;
                }
                var ff = user.Schedules.Where(m=>m.WaitPaymentDate != DateTime.MinValue).ToList();
                //var ff = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == user.UserId && m.WaitPaymentDate != DateTime.MinValue);
                //var ff = TestData.Schedules.Where(m => m.UserId == user.UserId && m.WaitPaymentDate != DateTime.MinValue).ToList();
                if (ff.Count > 0)
                {
                    foreach (var item in ff)
                    {
                        item.WaitPaymentDate = DateTime.MinValue;
                    }
                }
                
                //await _scheduleRepository.UpdateRange(ff);
                var list = user.Schedules.Where(m=> m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue).ToList();
                //var list = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == Convert.ToInt32(user.UserId) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue);
                list.Reverse();
                //var list = TestData.Schedules.Where(m => m.UserId == Convert.ToInt32(user.UserId) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue).Reverse().ToList();
                foreach (var item in list)
                {
                    if (item.WaitPaymentDate != DateTime.MinValue)
                    {
                        item.WaitPaymentDate = DateTime.MinValue;
                    }
                }

                var sorted = ScheduleService.SortSchedulesForUnpaid(_mapper.Map<List<ScheduleDTO>>(list));

                foreach (var item in sorted)
                {
                    var sch2 = user.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);
                    //var sch2 = TestData.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);

                    sch2.WaitPaymentDate = item.Nearest;

                    //await _scheduleRepository.Update(sch2);
                }
                await _userRepository.SaveChanges(user);

                var manager = (await _userRepository.GetManagerId());
                await NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT, user.UserId.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);
                await NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT_FOR_MANAGER.Replace("{name}",
                    user.FirstName + " " + user.LastName),manager.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);

            }

        }
    }
}

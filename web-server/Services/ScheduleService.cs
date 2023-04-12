using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class SortedModel
    {
        public int ScheduleId { get; set; }
        public int TutorId { get; set; }
        public DateTime Nearest { get; set; }
    }
    public class ScheduleService : IScheduleService
    {
        ScheduleRepository _scheduleRepository;
        UserRepository _userRepository;
        NotificationRepository _notificationRepository;
        public ScheduleService(ScheduleRepository scheduleRepository, UserRepository userRepository, NotificationRepository notificationRepository)
        {
            _scheduleRepository = scheduleRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
        }
        public async Task<Schedule> AddScheduleFromUser(string args, IHubContext<NotifHub> _hubContext)
        {
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Registration>(args);
            var user = await _userRepository.GetUserById(model.UserId);
            //var user = TestData.UserList.FirstOrDefault(m => m.UserId == model.UserId);
            if (user.Role != "Student")
            {
                return null;
            }
            if (user.Credit.Where(m => m.Repaid == false).ToList().Count > 3)
            {
                return null;
            }

            var schedule = await _scheduleRepository.GetScheduleByFunc(m => m.StartDate.DayOfWeek == model.WantThis.First().dateTime.DayOfWeek && m.StartDate.ToString("HH:mm") == model.WantThis.First().dateTime.ToString("HH:mm"));
            //var schedule = TestData.Schedules.FirstOrDefault();
            schedule.Course = model.Course;
            schedule.UserId = model.UserId;
            schedule.WaitPaymentDate = user.LessonsCount > 0 ? DateTime.MinValue : model.WantThis.First().dateTime;
            schedule.Status = Status.Ожидает;
            schedule.UserName = user.FirstName + " " + user.LastName;
            schedule.CreatedDate = DateTime.Now;

            schedule.Looped = true;

            await _scheduleRepository.Update(schedule);

            var text = Constants.NOTIF_NEW_LESSON_TUTOR.Replace("{name}", user.FirstName + " " + user.LastName).Replace("{date}", schedule.StartDate.ToString("dd.MM.yyyy HH:mm"));
            NotifHub.SendNotification(text, model.TutorId.ToString(), _hubContext, _userRepository, _notificationRepository);
                        
            return schedule;
        }
        public async Task<string> ChangeStatus(string args, IHubContext<NotifHub> _hubContext)
        {
            var split = args.Split(';');
            var status = split[0];
            var tutor_id = Convert.ToInt32(split[1]);
            var user_id = Convert.ToInt32(split[2]);
            var date = DateTime.Parse(split[3]);
            var dateCurr = DateTime.Parse(split[4]);
            var warn = Convert.ToBoolean(split[5]);

            var model =  await _scheduleRepository.GetScheduleByFunc(m => m.TutorId == tutor_id && m.UserId == user_id && m.StartDate == date);
            //var model = TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == date);
            var user = await _userRepository.GetUserById(user_id);
            //var user = (Student)TestData.UserList.FirstOrDefault(m => m.UserId == user_id);
            var tutor = await _userRepository.GetUserById(tutor_id);
            //var tutor = (Tutor)TestData.UserList.FirstOrDefault(m => m.UserId == tutor_id);
            var schedule = await _scheduleRepository.GetScheduleByFunc(m => m.TutorId == tutor_id && m.UserId == user_id && m.StartDate == date);
            //var schedule = TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == date);
            var manager = await _userRepository.GetUserById(await _userRepository.GetManagerId());
            //var manager = TestData.UserList.FirstOrDefault(m => m.Role == "Manager");

            if ((Status)Enum.Parse(typeof(Status), status) == Status.Проведен && (user.LessonsCount == 0 || user.Credit.Where(m => m.Repaid == false).ToList().Count > 0))
            {
                return "Не удалось поменять статус занятия. Ученик не произвел оплату.";
            }

            int initPay = 0;
            if ((Status)Enum.Parse(typeof(Status), status) == Status.Проведен)
            {
                user.LessonsCount--;


                if (user.LessonsCount >= 0)
                {
                    user.BalanceHistory.Add(new BalanceHistory() { CustomMessage = $"-1 занятие с репетитором {tutor.FirstName} {tutor.LastName}" });


                    if (user.Money.Count > 0)
                    {
                        var for_tutor = 0.0;
                        var for_manager = 0.0;

                        var f = user.Money.OrderBy(m => m.Cost).ToList().Where(m => m.Count > 0);

                        foreach (var item in f)
                        {
                            if (item.Count != 0)
                            {
                                for_tutor = Math.Abs(item.Cost / 100 * 60);
                                for_manager = Math.Abs(item.Cost / 100 * 40);
                                user.Money.FirstOrDefault(m => m.Cost == item.Cost && m.Cost > 0).Count--;
                                schedule.PaidLessons.Add( new PaidLesson() {  PaidDate = dateCurr, PaidCount = (int)Math.Abs(item.Cost) });
                                break;
                            }
                        }

                        tutor.Balance += for_tutor;
                        tutor.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_tutor) }, CustomMessage = $"Оплата за проведенный урок. Студент: {user.FirstName} {user.LastName}" });

                        manager.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_manager) }, CustomMessage = $"Оплата за проведенный урок. Студент: {user.FirstName} {user.LastName}. Репетитор: {tutor.FirstName} {tutor.LastName}" });

                        manager.Balance += for_manager;
                      
                    }


                }

                schedule.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON).NotifValue = false;
                schedule.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON).NotifValue = false;
                schedule.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_DONT_FORGET_SET_STATUS).NotifValue = false;

                if (model.Looped)
                {
                    schedule.ReadyDates.Add(new ReadyDate() { Date = dateCurr });
                }
                else
                {
                    schedule.Status = (Status)Enum.Parse(typeof(Status), status);
                    schedule.EndDate = dateCurr;
                }
                await _scheduleRepository.Update(schedule);

                if (user.LessonsCount == 1)
                {
                    NotifHub.SendNotification(Constants.NOTIF_ONE_LESSON_LEFT, user_id.ToString(), _hubContext, _userRepository, _notificationRepository);
                }
                else if (user.LessonsCount <= 0)
                {
                    if (user.StartWaitPayment == DateTime.MinValue || user.StartWaitPayment == null)
                    {
                        user.StartWaitPayment = DateTime.Now;
                    }
                    var ff = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == user_id && m.WaitPaymentDate != DateTime.MinValue);
                    //var ff = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == user_id && m.WaitPaymentDate != DateTime.MinValue);
                    if (ff.Count > 0)
                    {
                        foreach (var item in ff)
                        {
                            item.WaitPaymentDate = DateTime.MinValue;
                        }
                    }
                    var sorted = SortSchedulesForUnpaid(await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == Convert.ToInt32(user_id) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue));
                    sorted.Reverse();

                    foreach (var item in sorted)
                    {

                        var sch = await _scheduleRepository.GetScheduleById(item.ScheduleId);//TestData.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);

                        sch.WaitPaymentDate = item.Nearest;

                        await _scheduleRepository.Update(sch);
                    }

                    NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT, user_id.ToString(), _hubContext, _userRepository, _notificationRepository);
                    NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT_FOR_MANAGER.Replace("{name}",
                        user.FirstName + " " + user.LastName), _userRepository.GetManagerId().ToString(), _hubContext, _userRepository, _notificationRepository);
                }




            }
            else if ((Status)Enum.Parse(typeof(Status), status) == Status.Пропущен)
            {


                if (warn)
                {
                    user.SkippedInThisMonth++;
                    if (user.SkippedInThisMonth == 3)
                    {
                        user.LessonsCount--;
                        warn = false;
                        if (user.LessonsCount >= 0)
                        {
                            user.BalanceHistory.Add(new BalanceHistory() { CustomMessage = $"-1 занятие с репетитором {tutor.FirstName} {tutor.LastName}" });


                            if (user.Money.Count > 0)
                            {
                                var for_tutor = 0.0;
                                var for_manager = 0.0;

                                var f = user.Money.OrderBy(m => m.Cost).ToList();

                                foreach (var item in f)
                                {
                                    if (item.Count != 0)
                                    {
                                        for_tutor = Math.Abs(item.Cost / 100 * 60);
                                        for_manager = Math.Abs(item.Cost / 100 * 40);
                                        user.Money.FirstOrDefault(m => m.Cost == item.Cost).Count--;
                                        initPay = (int)Math.Abs(item.Cost);
                                        break;
                                    }
                                }
                                
                                tutor.Balance += for_tutor;
                                tutor.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_tutor) }, CustomMessage = $"Оплата за 1 пропущенное занятие. Студент: {user.FirstName} {user.LastName}" });
                                manager.Balance += for_manager;
                                manager.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_manager) }, CustomMessage = $"Оплата за 1 пропущенное занятие. Студент: {user.FirstName} {user.LastName}. Репетитор: {tutor.FirstName} {tutor.LastName}" });


                            }


                        }
                        else
                        {
                            user.Credit.Add(new UserCredit() { Id = user.Credit.Where(m => m.Repaid == false).ToList().Count == 0 ? 0 : user.Credit.Last().Id + 1, Amount = 1000, TutorId = tutor_id, ScheduleId = schedule.Id, ScheduleSkippedDate = dateCurr });
                        }

                    }

                    if (user.SkippedInThisMonth == 1)
                    {

                        NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_LAST_ONE.
              Replace("{userName}", user.FirstName + " " + user.LastName).
              Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateCurr.ToString("dd.MM.yyyy HH:mm")),
              _userRepository.GetManagerId().ToString(), _hubContext, _userRepository, _notificationRepository);

                        NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_LAST_ONE.
                   Replace("{userName}", user.FirstName + " " + user.LastName).
                   Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateCurr.ToString("dd.MM.yyyy HH:mm")),
                  user_id.ToString(), _hubContext, _userRepository, _notificationRepository);

                        // уведомления что ученик пропустил. менеджеру и ученику. Осталось одно бесплатное


                    }

                    if (user.SkippedInThisMonth == 2)
                    {

                        NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_NO_SKIP.
                   Replace("{userName}", user.FirstName + " " + user.LastName),
                  _userRepository.GetManagerId().ToString(), _hubContext, _userRepository, _notificationRepository);

                        NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_NO_SKIP.
                   Replace("{userName}", user.FirstName + " " + user.LastName),
                  user_id.ToString(), _hubContext, _userRepository, _notificationRepository);
                    }

                    if (user.SkippedInThisMonth >= 3)
                    {

                        NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_WARN.
                      Replace("{userName}", user.FirstName + " " + user.LastName).
                      Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateCurr.ToString("dd.MM.yyyy HH:mm")),
                      _userRepository.GetManagerId().ToString(), _hubContext, _userRepository, _notificationRepository);

                        NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_WARN.
                   Replace("{userName}", user.FirstName + " " + user.LastName).
                   Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateCurr.ToString("dd.MM.yyyy HH:mm")),
                  user_id.ToString(), _hubContext, _userRepository, _notificationRepository);
                    }

                }
                else
                {

                    user.LessonsCount--;


                    NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_NO_WARN.
                  Replace("{userName}", user.FirstName + " " + user.LastName).
                  Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateCurr.ToString("dd.MM.yyyy HH:mm")),
                  _userRepository.GetManagerId().ToString(), _hubContext, _userRepository, _notificationRepository);

                    NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_NO_WARN.
               Replace("{userName}", user.FirstName + " " + user.LastName).
               Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateCurr.ToString("dd.MM.yyyy HH:mm")),
              user_id.ToString(), _hubContext, _userRepository, _notificationRepository);

                    // уведомление ученику и менеджеру что не предупредил

                    user.BalanceHistory.Add(new BalanceHistory() { CustomMessage = $"-1 занятие с репетитором {tutor.FirstName} {tutor.LastName}" });


                    if (user.Money.Where(m => m.Count > 0).ToList().Count > 0)
                    {
                        var for_tutor = 0.0;
                        var for_manager = 0.0;

                        var f = user.Money.OrderBy(m => m.Cost).ToList().Where(m=>m.Count > 0);

                        foreach (var item in f)
                        {
                            if (item.Count > 0)
                            {
                                for_tutor = Math.Abs(item.Cost / 100 * 60);
                                for_manager = Math.Abs(item.Cost / 100 * 40);
                                user.Money.FirstOrDefault(m => m.Cost == item.Cost && item.Count > 0).Count--;
                                initPay = (int)Math.Abs(item.Cost);
                                break;
                            }
                        }
                        tutor.Balance += for_tutor;
                        tutor.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_tutor) }, CustomMessage = $"Оплата за 1 пропущенное занятие. Студент: {user.FirstName} {user.LastName}" });

                        manager.Balance += for_manager;
                        manager.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_manager) }, CustomMessage = $"Оплата за 1 пропущенное занятие. Студент: {user.FirstName} {user.LastName}. Репетитор: {tutor.FirstName} {tutor.LastName}" });



                    }
                    else
                    {
                        user.Credit.Add(new UserCredit() { Id = user.Credit.Where(m => m.Repaid == false).ToList().Count == 0 ? 0 : user.Credit.Last().Id + 1, Amount = 1000, TutorId = tutor_id, ScheduleId = schedule.Id, ScheduleSkippedDate = dateCurr });
                    }
                }


                if (schedule.Looped)
                {
                    schedule.SkippedDates.Add(new SkippedDate() { Date = dateCurr , WasWarn = warn, InitPaid = initPay});
                }
                else
                {
                    schedule.SkippedDates.Add(new SkippedDate() { Date = dateCurr, WasWarn = warn, InitPaid = initPay });
                    schedule.Status = Status.Пропущен;
                }

                if (user.LessonsCount <= 0)
                {
                    if (user.StartWaitPayment == DateTime.MinValue || user.StartWaitPayment == null)
                    {
                        user.StartWaitPayment = DateTime.Now;
                    }

                    var ff = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == user_id && m.WaitPaymentDate != DateTime.MinValue);
                    if (ff.Count > 0)
                    {
                        foreach (var item in ff)
                        {
                            item.WaitPaymentDate = DateTime.MinValue;
                        }
                    }

                    var sorted = SortSchedulesForUnpaid(await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == Convert.ToInt32(user_id) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue));
                    sorted.Reverse();
                    //var sorted = SortSchedulesForUnpaid(TestData.Schedules.Where(m => m.UserId == Convert.ToInt32(user_id) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue).Reverse().ToList());


                    foreach (var item in sorted)
                    {

                        var sch = await _scheduleRepository.GetScheduleById(item.ScheduleId); // TestData.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);

                        sch.WaitPaymentDate = item.Nearest;

                        await _scheduleRepository.Update(sch);
                    }

                    NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT, user_id.ToString(), _hubContext, _userRepository, _notificationRepository);
                    NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT_FOR_MANAGER.Replace("{name}",
                        user.FirstName + " " + user.LastName), _userRepository.GetManagerId().ToString(), _hubContext, _userRepository, _notificationRepository);
                }

            }

            await _userRepository.Update(user);
            await _userRepository.Update(manager);
            await _userRepository.Update(tutor);
            await _scheduleRepository.Update(schedule);
            return "OK";
        }

        public async Task<List<Schedule>> GetAllSchedules()
        {

            return await _scheduleRepository.GetSchedulesByFunc(null);
        }
        public async Task<List<RescheduledLessons>> GetAllReschedules()
        {
            return await _scheduleRepository.GetReschedulesByFunc(null);
        }

        public async Task<Schedule> GetScheduleById(int id)
        {
            return await _scheduleRepository.GetScheduleById(id);
        }
        public async Task<bool> Update(Schedule schedule)
        {
            await _scheduleRepository.Update(schedule);
            return true;
        }
        public async Task<List<Schedule>> GetSchedules(string args)
        {
            var user = await _userRepository.GetUserByToken(args);
            if (user == null)
            {
                return null;
            }

           var schedules = new List<Schedule>();
          //  schedules = user.Schedules;
            if (user.Role == "Student")
            {
               // schedules = user.Schedules;
                schedules = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == user.UserId);

            }
            else
            {

                schedules = await _scheduleRepository.GetSchedulesByFunc(m => m.TutorId == user.UserId);
            }


            return schedules;
        }

        public static List<SortedModel> SortSchedulesForUnpaid(List<Schedule> schedules2)
        {
            Dictionary<int, List<Schedule>> curr = new Dictionary<int, List<Schedule>>();

            foreach (var item in schedules2)
            {
                if (!curr.ContainsKey(item.TutorId))
                {
                    curr.Add(item.TutorId, new List<Schedule>() { item });
                }
                else
                {
                    curr[item.TutorId].Add(item);
                }
            }

            var result = new List<SortedModel>();
            var date2 = DateTime.MinValue;

            foreach (var item in curr)
            {
                var model3 = new SortedModel() { TutorId = -1 };
                foreach (var cur in item.Value)
                {

                    if (model3.TutorId == -1)
                    {

                        if (cur.Looped)
                        {

                            if (cur.ReadyDates.Count > 0)
                            {
                                date2 = cur.ReadyDates.Last().Date.AddDays(7);
                                model3.TutorId = item.Key;
                                model3.ScheduleId = cur.Id;
                                model3.Nearest = date2;
                            }

                            if (cur.RescheduledLessons.Count > 0)
                            {
                                if (date2 < cur.RescheduledLessons.Last().NewTime)
                                {
                                    date2 = cur.RescheduledLessons.Last().NewTime;
                                    model3.TutorId = item.Key;
                                    model3.ScheduleId = cur.Id;
                                    model3.Nearest = date2;
                                }
                            }
                            if (cur.RescheduledDate != DateTime.MinValue)
                            {
                                if (date2 < cur.RescheduledDate)
                                {
                                    date2 = cur.RescheduledDate;
                                    model3.TutorId = item.Key;
                                    model3.ScheduleId = cur.Id;
                                    model3.Nearest = date2;
                                }
                            }

                            if (cur.SkippedDates.Count > 0)
                            {
                                if (date2 < cur.SkippedDates.Last().Date.AddDays(7))
                                {
                                    date2 = cur.SkippedDates.Last().Date.AddDays(7);
                                    model3.TutorId = item.Key;
                                    model3.ScheduleId = cur.Id;
                                    model3.Nearest = date2;
                                }

                            }

                            if (date2 < cur.StartDate)
                            {
                                date2 = cur.StartDate;
                                model3.TutorId = item.Key;
                                model3.ScheduleId = cur.Id;
                                model3.Nearest = date2;

                            }
                        }
                        else
                        {
                            if (cur.Status == Status.Ожидает)
                            {
                                date2 = cur.StartDate;
                                model3.TutorId = item.Key;
                                model3.ScheduleId = cur.Id;
                                model3.Nearest = date2;
                            }

                        }


                    }
                    else
                    {
                        if (cur.Looped)
                        {
                            if (cur.ReadyDates.Count > 0)
                            {
                                if ((cur.ReadyDates.Last().Date.AddDays(7) - DateTime.Now).Duration() < (date2 - DateTime.Now).Duration())
                                {
                                    date2 = cur.ReadyDates.Last().Date.AddDays(7);
                                    model3.TutorId = item.Key;
                                    model3.ScheduleId = cur.Id;
                                    model3.Nearest = date2;
                                }
                            }

                            if (cur.RescheduledLessons.Count > 0)
                            {
                                if ((cur.RescheduledLessons.Last().NewTime - DateTime.Now).Duration() < (date2 - DateTime.Now).Duration())
                                {
                                    date2 = cur.RescheduledLessons.Last().NewTime;
                                    model3.TutorId = item.Key;
                                    model3.ScheduleId = cur.Id;
                                    model3.Nearest = date2;
                                }
                            }
                            if (cur.RescheduledDate != DateTime.MinValue)
                            {
                                if ((cur.RescheduledDate - DateTime.Now).Duration() < (date2 - DateTime.Now).Duration())
                                {
                                    date2 = cur.RescheduledDate;
                                    model3.TutorId = item.Key;
                                    model3.ScheduleId = cur.Id;
                                    model3.Nearest = date2;
                                }
                            }

                            if (cur.SkippedDates.Count > 0)
                            {
                                if ((cur.SkippedDates.Last().Date.AddDays(7) - DateTime.Now).Duration() < (date2 - DateTime.Now).Duration())
                                {
                                    date2 = cur.SkippedDates.Last().Date.AddDays(7);
                                    model3.TutorId = item.Key;
                                    model3.ScheduleId = cur.Id;
                                    model3.Nearest = date2;
                                }

                            }

                        }
                        else
                        {
                            if (((cur.StartDate - DateTime.Now).Duration() < (date2 - DateTime.Now).Duration()) && cur.Status == Status.Ожидает)
                            {

                                date2 = cur.StartDate;
                                model3.TutorId = item.Key;
                                model3.ScheduleId = cur.Id;
                                model3.Nearest = date2;

                            }
                        }
                    }
                }

                result.Add(model3);
            }

            return result;
        }

        public async Task<Schedule> UpdateRange(List<Schedule> schedules)
        {
            await _scheduleRepository.UpdateRange(schedules);
            return null;
        }
    }
}

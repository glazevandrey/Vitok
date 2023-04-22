using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Database.Repositories;
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
        IMapper _mapper;
        public ScheduleService(IMapper mapper, ScheduleRepository scheduleRepository, UserRepository userRepository, NotificationRepository notificationRepository)
        {
            _mapper = mapper;
            _scheduleRepository = scheduleRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
        }
        public async Task<Schedule> AddScheduleFromUser(string args, IHubContext<NotifHub> _hubContext)
        {
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Registration>(args);
            var user = await _userRepository.GetStudent(model.ExistUserId);
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
            schedule.CourseId = _mapper.Map<CourseDTO>(model.Course).Id;
            //schedule.Course =  _mapper.Map<CourseDTO>(model.Course);
            schedule.UserId = model.ExistUserId;
            schedule.WaitPaymentDate = user.LessonsCount > 0 ? DateTime.MinValue : model.WantThis.First().dateTime;
            schedule.Status = Status.Ожидает;
            schedule.UserName = user.FirstName + " " + user.LastName;
            schedule.CreatedDate = DateTime.Now;

            schedule.Looped = true;

            await _scheduleRepository.Update(schedule);

            var tutor = await _userRepository.GetTutor(model.TutorId);
            //var tutor = (Tutor)await _userRepository.GetUserById(model.TutorId);

            if (tutor.Chat != null && tutor.Chat?.Contacts?.FirstOrDefault(m => m.UserId == model.ExistUserId) == null)
            {
                tutor.Chat.Contacts.Add(new Contact() { UserId = model.ExistUserId });
            }
            if (user.Chat != null && user.Chat?.Contacts?.FirstOrDefault(m => m.UserId == model.ExistUserId) == null)
            {
                user.Chat.Contacts.Add(new Contact() { UserId = model.TutorId });
            }

            var rem = tutor.UserDates.FirstOrDefault(m => m.dateTime == model.WantThis.FirstOrDefault().dateTime);
            tutor.UserDates.Remove(rem);
            await _userRepository.SaveChanges(tutor);
            await _userRepository.SaveChanges(user);

            var text = Constants.NOTIF_NEW_LESSON_TUTOR.Replace("{name}", user.FirstName + " " + user.LastName).Replace("{date}", schedule.StartDate.ToString("dd.MM.yyyy HH:mm"));
            await NotifHub.SendNotification(text, model.TutorId.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);

            return new Schedule();
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

            var user = await _userRepository.GetStudent(user_id);
            var tutor = await _userRepository.GetTutor(tutor_id);
            var manager = await _userRepository.GetUserById(await _userRepository.GetManagerId());

            var schedule = tutor.Schedules.FirstOrDefault(m => m.StartDate == date);

            if ((Status)Enum.Parse(typeof(Status), status) == Status.Проведен && (user.LessonsCount <= 0 || user.Credit.Where(m => m.Repaid == false).ToList().Count > 0))
            {
                return "Не удалось поменять статус занятия. Ученик не произвел оплату.";
            }


            if ((Status)Enum.Parse(typeof(Status), status) == Status.Проведен)
            {
                await ChangeStatusToReady(tutor, user, manager, schedule, dateCurr, date, status);
            }
            else if ((Status)Enum.Parse(typeof(Status), status) == Status.Пропущен)
            {
                await ChangeStatusToSkipped(user, tutor, manager, schedule, dateCurr, warn);
            }


            if (user.LessonsCount <= 0)
            {
                await CalculateNoPaidWarn(user);
            }

            await _userRepository.SaveChanges(tutor);
            await _userRepository.SaveChanges(user);
            await _userRepository.Update(manager);

            if (user.LessonsCount == 1)
            {
                await NotifHub.SendNotification(Constants.NOTIF_ONE_LESSON_LEFT, user.UserId.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);
            }

            if (user.SkippedInThisMonth == 1)
            {

                await NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_LAST_ONE.
      Replace("{userName}", user.FirstName + " " + user.LastName).
      Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateCurr.ToString("dd.MM.yyyy HH:mm")),
      manager.UserId.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);

                await NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_LAST_ONE.
           Replace("{userName}", user.FirstName + " " + user.LastName).
           Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateCurr.ToString("dd.MM.yyyy HH:mm")),
          user_id.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);

                // уведомления что ученик пропустил. менеджеру и ученику. Осталось одно бесплатное


            }

            if (user.SkippedInThisMonth == 2)
            {

                await NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_NO_SKIP.
           Replace("{userName}", user.FirstName + " " + user.LastName),
         manager.UserId.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);

                await NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_NO_SKIP.
           Replace("{userName}", user.FirstName + " " + user.LastName),
          user_id.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);
            }

            if (user.SkippedInThisMonth >= 3)
            {

                await NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_WARN.
              Replace("{userName}", user.FirstName + " " + user.LastName).
              Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateCurr.ToString("dd.MM.yyyy HH:mm")),
              manager.UserId.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);

                await NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_WARN.
           Replace("{userName}", user.FirstName + " " + user.LastName).
           Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateCurr.ToString("dd.MM.yyyy HH:mm")),
          user_id.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);
            }

            if (!warn)
            {
                await NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_NO_WARN.
              Replace("{userName}", user.FirstName + " " + user.LastName).
              Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateCurr.ToString("dd.MM.yyyy HH:mm")),
              manager.UserId.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);

                await NotifHub.SendNotification(Constants.NOTIF_USER_SKIPP_NO_WARN.
           Replace("{userName}", user.FirstName + " " + user.LastName).
           Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", dateCurr.ToString("dd.MM.yyyy HH:mm")),
          user_id.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);

            }

            if (user.LessonsCount <= 0)
            {
                await NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT, user_id.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);
                await NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT_FOR_MANAGER.Replace("{name}",
                    user.FirstName + " " + user.LastName), manager.UserId.ToString(), _hubContext, _userRepository, _notificationRepository, _mapper);
            }

            return "OK";
        }

        public async Task<List<ScheduleDTO>> GetAllSchedules() =>
            await _scheduleRepository.GetSchedulesByFunc(null);

        public async Task<List<RescheduledLessons>> GetAllReschedules() =>
            await _scheduleRepository.GetReschedulesByFunc(null);

        public async Task<ScheduleDTO> GetScheduleById(int id) =>
            await _scheduleRepository.GetScheduleById(id);

        public async Task<bool> Update(ScheduleDTO schedule)
        {
            try
            {
                await _scheduleRepository.Update(schedule);

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return true;
        }

        public async Task<List<ScheduleDTO>> GetSchedules(string args)
        {
            var user = await _userRepository.GetUserByToken(args);
            if (user == null)
            {
                return null;
            }

            var schedules = new List<ScheduleDTO>();

            if (user.Role == "Student")
            {
                schedules = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == user.UserId);
            }
            else
            {
                schedules = await _scheduleRepository.GetSchedulesByFunc(m => m.TutorId == user.UserId);
            }


            return schedules;
        }

        private async Task CalculateNoPaidWarn(StudentDTO user)
        {
            if (user.LessonsCount <= 0)
            {
                if (user.StartWaitPayment == DateTime.MinValue || user.StartWaitPayment == null)
                {
                    user.StartWaitPayment = DateTime.Now;
                }

                var list = user.Schedules.Where(m => m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue);
                var sorted = SortSchedulesForUnpaid(_mapper.Map<List<ScheduleDTO>>(list));
                sorted.Reverse();
                //var sorted = SortSchedulesForUnpaid(TestData.Schedules.Where(m => m.UserId == Convert.ToInt32(user_id) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue).Reverse().ToList());

                foreach (var item in user.Schedules)
                {
                    item.WaitPaymentDate = DateTime.MinValue;
                    await _scheduleRepository.Update(item);
                }

                foreach (var item in sorted)
                {
                    var sch = await _scheduleRepository.GetScheduleById(item.ScheduleId);
                    //var sch = user.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId); // await _scheduleRepository.GetScheduleById(item.ScheduleId); // TestData.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);

                    sch.WaitPaymentDate = item.Nearest;

                    await _scheduleRepository.Update(sch);

                }
            }

        }
        public static List<SortedModel> SortSchedulesForUnpaid(List<ScheduleDTO> schedules2)
        {
            Dictionary<int, List<ScheduleDTO>> curr = new Dictionary<int, List<ScheduleDTO>>();

            foreach (var item in schedules2)
            {
                if (!curr.ContainsKey(item.TutorId))
                {
                    curr.Add(item.TutorId, new List<ScheduleDTO>() { item });
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
                                continue;
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

                            if (date2 > cur.StartDate && cur.ReadyDates?.FirstOrDefault(m => m.Date == cur.StartDate) == null && cur.SkippedDates?.FirstOrDefault(m => m.Date == cur.StartDate) == null)
                            {
                                date2 = cur.StartDate;
                                model3.TutorId = item.Key;
                                model3.ScheduleId = cur.Id;
                                model3.Nearest = date2;

                            }

                        }
                        else
                        {

                            // когда стартдейт больше на 3 дня , dur1 > dur2
                            //var dur1 = (cur.StartDate - DateTime.Now).Duration();
                            //var dur2 = (date2 - DateTime.Now).Duration();
                            if ((date2 > cur.StartDate) && cur.Status == Status.Ожидает)
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

        private async Task ChangeStatusToSkipped(StudentDTO user, TutorDTO tutor, User manager, ScheduleDTO schedule,
            DateTime dateCurr, bool warn)
        {
            int initPay = 0;

            if (schedule.Looped)
            {
                schedule.SkippedDates.Add(new SkippedDate() { Date = dateCurr, WasWarn = warn, InitPaid = initPay });
            }
            else
            {
                schedule.SkippedDates.Add(new SkippedDate() { Date = dateCurr, WasWarn = warn, InitPaid = initPay });
                schedule.Status = Status.Пропущен;
            }

            schedule.WaitPaymentDate = DateTime.MinValue;
            await _scheduleRepository.Update(schedule);

            if (warn)
            {
                user.SkippedInThisMonth++;
                if (user.SkippedInThisMonth == 3)
                {
                    user.LessonsCount--;
                    warn = false;
                    if (user.LessonsCount >= 0)
                    {

                        MakePayment(user, tutor, manager, ref initPay, Status.Пропущен, null, DateTime.MinValue,
                             $"-1 занятие с репетитором {tutor.FirstName} {tutor.LastName}",
                             $"Оплата за 1 пропущенное занятие. Студент: {user.FirstName} {user.LastName}",
                             $"Оплата за 1 пропущенное занятие. Студент: {user.FirstName} {user.LastName}. Репетитор: {tutor.FirstName} {tutor.LastName}");


                        //user.BalanceHistory.Add(new BalanceHistory() { CustomMessage = $"-1 занятие с репетитором {tutor.FirstName} {tutor.LastName}" });
                        //if (user.Money.Count > 0)
                        //{
                        //    var for_tutor = 0.0;
                        //    var for_manager = 0.0;

                        //    var f = user.Money.OrderBy(m => m.Cost).ToList();

                        //    foreach (var item in f)
                        //    {
                        //        if (item.Count != 0)
                        //        {
                        //            for_tutor = Math.Abs(item.Cost / 100 * 60);
                        //            for_manager = Math.Abs(item.Cost / 100 * 40);
                        //            user.Money.FirstOrDefault(m => m.Cost == item.Cost).Count--;
                        //            initPay = (int)Math.Abs(item.Cost);
                        //            break;
                        //        }
                        //    }

                        //    tutor.Balance += for_tutor;
                        //    tutor.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_tutor) }, CustomMessage = $"Оплата за 1 пропущенное занятие. Студент: {user.FirstName} {user.LastName}" });
                        //    manager.Balance += for_manager;
                        //    manager.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_manager) }, CustomMessage = $"Оплата за 1 пропущенное занятие. Студент: {user.FirstName} {user.LastName}. Репетитор: {tutor.FirstName} {tutor.LastName}" });


                        //}


                    }
                    else
                    {
                        user.Credit.Add(new UserCredit() { Amount = 1000, TutorId = tutor.UserId, ScheduleId = schedule.Id, ScheduleSkippedDate = dateCurr });
                    }

                }


            }
            else
            {

                user.LessonsCount--;


                // уведомление ученику и менеджеру что не предупредил



                if (user.Money.Where(m => m.Count > 0).ToList().Count > 0)
                {
                    MakePayment(user, tutor, manager, ref initPay, Status.Пропущен, null, DateTime.MinValue,
                        $"-1 занятие с репетитором {tutor.FirstName} {tutor.LastName}",
                        $"Оплата за 1 пропущенное занятие. Студент: {user.FirstName} {user.LastName}",
                        $"Оплата за 1 пропущенное занятие. Студент: {user.FirstName} {user.LastName}. Репетитор: {tutor.FirstName} {tutor.LastName}");


                    //var for_tutor = 0.0;
                    //var for_manager = 0.0;

                    //var f = user.Money.OrderBy(m => m.Cost).ToList().Where(m => m.Count > 0);

                    //foreach (var item in f)
                    //{
                    //    if (item.Count > 0)
                    //    {
                    //        for_tutor = Math.Abs(item.Cost / 100 * 60);
                    //        for_manager = Math.Abs(item.Cost / 100 * 40);
                    //        user.Money.FirstOrDefault(m => m.Cost == item.Cost && item.Count > 0).Count--;
                    //        initPay = (int)Math.Abs(item.Cost);
                    //        break;
                    //    }
                    //}
                    //tutor.Balance += for_tutor;
                    //tutor.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_tutor) }, CustomMessage = $"Оплата за 1 пропущенное занятие. Студент: {user.FirstName} {user.LastName}" });

                    //manager.Balance += for_manager;
                    //manager.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_manager) }, CustomMessage = $"Оплата за 1 пропущенное занятие. Студент: {user.FirstName} {user.LastName}. Репетитор: {tutor.FirstName} {tutor.LastName}" });



                }
                else
                {
                    user.BalanceHistory.Add(new BalanceHistory() { CustomMessage = $"-1 занятие с репетитором {tutor.FirstName} {tutor.LastName}" });
                    user.Credit.Add(new UserCredit() { Amount = 1000, TutorId = tutor.UserId, ScheduleId = schedule.Id, ScheduleSkippedDate = dateCurr });
                }


            }

        }

        private void MakePayment(StudentDTO user, TutorDTO tutor, User manager, ref int initPay, Status status, ScheduleDTO schedule, DateTime dateCurr,
            string userMessage, string tutorMesasge, string managerMessage)
        {

            user.BalanceHistory.Add(new BalanceHistory() { CustomMessage = userMessage });

            var for_tutor = 0.0;
            var for_manager = 0.0;

            var f = user.Money.OrderBy(m => m.Cost).ToList().Where(m => m.Count > 0);

            foreach (var item in f)
            {
                if (item.Count > 0)
                {
                    for_tutor = Math.Abs(item.Cost / 100 * 60);
                    for_manager = Math.Abs(item.Cost / 100 * 40);
                    user.Money.FirstOrDefault(m => m.Cost == item.Cost && item.Count > 0).Count--;
                    if (status == Status.Пропущен)
                    {
                        initPay = (int)Math.Abs(item.Cost);
                    }
                    else if (status == Status.Проведен)
                    {
                        schedule.PaidLessons.Add(new PaidLesson() { PaidDate = dateCurr, PaidCount = (int)Math.Abs(item.Cost) });
                    }

                    break;
                }
            }
            tutor.Balance += for_tutor;
            tutor.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_tutor) }, CustomMessage = tutorMesasge });

            manager.Balance += for_manager;
            manager.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_manager) }, CustomMessage = managerMessage });

        }

        private async Task ChangeStatusToReady(TutorDTO tutor, StudentDTO user, User manager, ScheduleDTO schedule,
            DateTime dateCurr, DateTime date, string status)
        {
            user.LessonsCount--;


            if (user.LessonsCount >= 0)
            {
                int x = 0;
                MakePayment(user, tutor, manager, ref x, Status.Проведен, schedule, dateCurr,
                    $"-1 занятие с репетитором {tutor.FirstName} {tutor.LastName}",
                    $"Оплата за проведенный урок. Студент: {user.FirstName} {user.LastName}",
                    $"Оплата за проведенный урок. Студент: {user.FirstName} {user.LastName}. Репетитор: {tutor.FirstName} {tutor.LastName}");

                //user.BalanceHistory.Add(new BalanceHistory() { CustomMessage = $"-1 занятие с репетитором {tutor.FirstName} {tutor.LastName}" });


                //if (user.Money.Count > 0)
                //{
                //    var for_tutor = 0.0;
                //    var for_manager = 0.0;

                //    var f = user.Money.OrderBy(m => m.Cost).ToList().Where(m => m.Count > 0);

                //    foreach (var item in f)
                //    {
                //        if (item.Count != 0)
                //        {
                //            for_tutor = Math.Abs(item.Cost / 100 * 60);
                //            for_manager = Math.Abs(item.Cost / 100 * 40);
                //            user.Money.FirstOrDefault(m => m.Cost == item.Cost && m.Cost > 0).Count--;
                //            schedule.PaidLessons.Add(new PaidLesson() { PaidDate = dateCurr, PaidCount = (int)Math.Abs(item.Cost) });
                //            break;
                //        }
                //    }

                //    tutor.Balance += for_tutor;
                //    tutor.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_tutor) }, CustomMessage = $"Оплата за проведенный урок. Студент: {user.FirstName} {user.LastName}" });
                //    manager.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(for_manager) }, CustomMessage = $"Оплата за проведенный урок. Студент: {user.FirstName} {user.LastName}. Репетитор: {tutor.FirstName} {tutor.LastName}" });

                //    manager.Balance += for_manager;

                //}


            }

            schedule.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON && m.Id != 0).NotifValue = false;
            schedule.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON && m.Id != 0).NotifValue = false;
            schedule.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_DONT_FORGET_SET_STATUS && m.Id != 0).NotifValue = false;

            if (user.Schedules.FirstOrDefault(m => m.StartDate == date && m.RemoveDate == DateTime.MinValue).Looped)
            {
                schedule.ReadyDates.Add(new ReadyDate() { Date = dateCurr });
            }
            else
            {
                schedule.Status = (Status)Enum.Parse(typeof(Status), status);
                schedule.EndDate = dateCurr;
            }

            await _scheduleRepository.Update(schedule);

        }
    }
}

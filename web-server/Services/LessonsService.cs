using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using web_server.DbContext;
using web_server.Models;
using web_server.Services.Interfaces;

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
                    var id = user.Money.FirstOrDefault() != null ? user.Money.FirstOrDefault().Id : 0;

                    user.Money.Add(new UserMoney() { Id = id, Cost = one, Count = lessonCount });
                }
                else
                {
                    user.Money.FirstOrDefault(m => m.Cost == one).Count += lessonCount;
                }

                TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).BalanceHistory.Add(new BalanceHistory()
                { CashFlow = new CashFlow() { Amount = (int)Math.Abs(tariff.Amount), Count = lessonCount} ,  CustomMessages = new CustomMessage() { MessageValue = $"Оплата тарифа: {tariff.Title}" } });

            }
            else
            {
                if (isTrial)
                {
                    TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).BalanceHistory.Add(new BalanceHistory() { Date = DateTime.Now.AddDays(2), CashFlow = new CashFlow() { Amount = 250, Count = 1 }, CustomMessages = new CustomMessage() { MessageValue = $"Оплачено пробное занятие" } });
                }
                else
                {
                    TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = 1000 * lessonCount, Count = lessonCount }, CustomMessages = new CustomMessage() { MessageValue = $"Оплачено занятий: {lessonCount}" } });
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


            var manager = TestData.Managers.First();

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

                        var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == user.Credit.First().TutorId);

                        var f_tut = Math.Abs(item.Cost / 100 * 60);
                        var f_manag = Math.Abs(item.Cost / 100 * 40);

                        tutor.Balance += f_tut;
                        tutor.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(f_tut) }, CustomMessages = new CustomMessage() { MessageValue = $"Оплата долга за 1 занятие. Студент: {user.FirstName} {user.LastName}" } });



                        manager.Balance += f_manag;
                        manager.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = (int)Math.Abs(f_manag) }, CustomMessages = new CustomMessage() { MessageValue = $"Оплата долга за 1 занятие. Студент: {user.FirstName} {user.LastName}. Репетитор: {tutor.FirstName} {tutor.LastName}" } });

                        how_minus++;
                        user.BalanceHistory.Add(new BalanceHistory() { CustomMessages = new CustomMessage() { MessageValue = $"Погашен долг за 1 занятие с репетитором {tutor.FirstName} {tutor.LastName}" } });

                        var credit = user.Credit.Where(m => m.Repaid == false).First();
                        credit.Repaid = true;
                        credit.Amount = item.Cost;
                    }

                    item.Count -= how_minus;

                }

            }

            var waited = schedules.Where(m => m.WaitPaymentDate != DateTime.MinValue).ToList();
            foreach (var item in waited)
            {
                TestData.Schedules.FirstOrDefault(m => m.Id == item.Id).WaitPaymentDate = DateTime.MinValue;
            }

            if (user.Credit.Where(m => m.Repaid == false).ToList().Count == 0)
            {
                user.StartWaitPayment = DateTime.MinValue;
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






            if (Convert.ToBoolean(loop))
            {
                var alredyUsed = TestData.Schedules.Where(m => m.TutorId == tutor_id && m.StartDate.DayOfWeek == newDateTime.DayOfWeek && m.StartDate.Hour == newDateTime.Hour).ToList();
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
                    Course = TestData.Courses.FirstOrDefault(m => m.Id == courseId),
                    Date = new UserDate() { dateTimes = new List<DateTime>() { newDateTime } },
                    Id = TestData.Schedules.Last().Id + 1,
                    StartDate = newDateTime,
                    Looped = true,
                };

                if (TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).WaitPaymentDate != DateTime.MinValue)
                {
                    new_model.WaitPaymentDate = TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).WaitPaymentDate;
                }
                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).Status = Status.Перенесен;
                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).RescheduledDate = cureDate;
                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).NewDate = newDateTime;

                TestData.Schedules.Add(new_model);

                // отправка манагеру что постоянный перенос
                NotifHub.SendNotification(Constants.NOTIF_REGULAR_RESCHEDULE.Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName)
                    .Replace("{studentName}", user.FirstName + " " + user.LastName)
                    .Replace("{oldDate}", cureDate.ToString("dd.MM.yyyy HH:mm"))
                    .Replace("{newDate}", newDateTime.ToString("dd.MM.yyyy HH:mm")), TestData.Managers.First().UserId.ToString(), _hubContext);


                // отправка студенту что перенос занятия
                NotifHub.SendNotification(Constants.NOTIF_LESSON_WAS_RESCHEDULED_FOR_STUDENT_REGULAR
                    .Replace("{name}", tutor.FirstName + " " + tutor.LastName)
                    .Replace("{dateOld}", cureDate.ToString("dd.MM.yyyy HH:mm"))
                    .Replace("{dateNew}", newDateTime.ToString("dd.MM.yyyy HH:mm")), user_id.ToString(), _hubContext);

                CalculateNoPaidWarn(user, _hubContext);

                return new_model;
            }
            else
            {
                var alredyUsed = TestData.Schedules.Where(m => m.TutorId == tutor_id && m.StartDate == newDateTime).ToList();
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
                if (TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).Status == Status.ОжидаетОплату)
                {
                    new_model.Status = Status.ОжидаетОплату;
                }
                else
                {
                    TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).Status = Status.Перенесен; ;

                }

                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).RescheduledLessons.Add(re_less);

                TestData.Schedules.Add(new_model);

                NotifHub.SendNotification(Constants.NOTIF_LESSON_WAS_RESCHEDULED_FOR_STUDENT
                   .Replace("{name}", tutor.FirstName + " " + tutor.LastName)
                   .Replace("{dateOld}", cureDate.ToString("dd.MM.yyyy HH:mm"))
                   .Replace("{dateNew}", newDateTime.ToString("dd.MM.yyyy HH:mm")), user_id.ToString(), _hubContext);

                // отправка манагеру что разовый перенос
                NotifHub.SendNotification(Constants.NOTIF_RESCHEDULE.Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName)
                    .Replace("{studentName}", user.FirstName + " " + user.LastName)
                    .Replace("{oldDate}", cureDate.ToString("dd.MM.yyyy HH:mm"))
                    .Replace("{newDate}", newDateTime.ToString("dd.MM.yyyy HH:mm")), TestData.Managers.First().UserId.ToString(), _hubContext);



                if (user.LessonsCount <= 0)
                {
                    if (user.StartWaitPayment == DateTime.MinValue || user.StartWaitPayment == null)
                    {
                        user.StartWaitPayment = DateTime.Now;
                    }
                    var ff = TestData.Schedules.Where(m => m.UserId == user_id && m.WaitPaymentDate != DateTime.MinValue).ToList();
                    if (ff.Count > 0)
                    {
                        foreach (var item in ff)
                        {
                            item.WaitPaymentDate = DateTime.MinValue;
                        }
                    }


                    NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT, user_id.ToString(), _hubContext);
                    NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT_FOR_MANAGER.Replace("{name}",
                        user.FirstName + " " + TestData.UserList.FirstOrDefault(m => m.UserId == user_id).LastName), TestData.Managers.First().UserId.ToString(), _hubContext);
                }

                CalculateNoPaidWarn(user, _hubContext);
                return new_model;
            }
        }
        public void CalculateNoPaidWarn(User user, IHubContext<NotifHub> _hubContext)
        {

            if (user.LessonsCount <= 0)
            {
                if (user.StartWaitPayment == DateTime.MinValue || user.StartWaitPayment == null)
                {
                    user.StartWaitPayment = DateTime.Now;
                }
                var ff = TestData.Schedules.Where(m => m.UserId == user.UserId && m.WaitPaymentDate != DateTime.MinValue).ToList();
                if (ff.Count > 0)
                {
                    foreach (var item in ff)
                    {
                        item.WaitPaymentDate = DateTime.MinValue;
                    }
                }

                var list = TestData.Schedules.Where(m => m.UserId == Convert.ToInt32(user.UserId) && m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue).Reverse().ToList();
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

                NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT, user.UserId.ToString(), _hubContext);
                NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT_FOR_MANAGER.Replace("{name}",
                    user.FirstName + " " + TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).LastName), TestData.Managers.First().UserId.ToString(), _hubContext);
            }

        }
    }
}

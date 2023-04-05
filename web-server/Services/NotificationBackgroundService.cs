using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using web_server.DbContext;
using web_server.Models;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IHubContext<NotifHub> _hubContext;
        ISenderService _senderService;
        public NotificationBackgroundService(IHubContext<NotifHub> hubContext, ISenderService senderService)
        {
            _hubContext = hubContext;
            _senderService = senderService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(10000);
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        Program.BackInAir = true;
                        // Получаем все занятия из базы данных
                        var lessons = TestData.Schedules;

                        // Обрабатываем каждое занятие
                        foreach (var lesson in lessons)
                        {
                            if (lesson.UserId == -1)
                            {
                                continue;
                            }
                            var lessonDate = new DateTime();

                            if (lesson.StartDate == lessonDate)
                            {
                                lessonDate = lesson.Date.dateTimes[0];
                            }
                            else
                            {
                                lessonDate = lesson.StartDate;
                            }


                            // Проверяем статус занятия
                            if (lesson.Status != Models.Status.Проведен)
                            {
                                if (!lesson.Looped)
                                {
                                    if (lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_DONT_FORGET_SET_STATUS).NotifValue == false)
                                    {
                                        TimeSpan timeLeft = DateTime.Now - lessonDate; // время после наачала занятия
                                        if (timeLeft.TotalMinutes > 61)
                                        {
                                            TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_DONT_FORGET_SET_STATUS).NotifValue = true;
                                            lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_DONT_FORGET_SET_STATUS).NotifValue = true;
                                            var email = TestData.UserList.FirstOrDefault(m => m.UserId == lesson.TutorId).Email;
                                            _senderService.SendMessage(email, Constants.NOTIF_DONT_FORGET_SET_STATUS);

                                            NotifHub.SendNotification(Constants.NOTIF_DONT_FORGET_SET_STATUS, lesson.TutorId.ToString(), _hubContext);
                                        }

                                    }
                                    if (lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON).NotifValue == false)
                                    {
                                        TimeSpan timeLeft = lessonDate - DateTime.Now; // Время, оставшееся до конца занятия

                                        if (timeLeft.TotalHours < 25 && timeLeft.TotalHours > 24)
                                        {
                                            TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON).NotifValue = true;
                                            lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON).NotifValue = true;



                                            NotifHub.SendNotification(Constants.NOTIF_TOMORROW_LESSON, lesson.TutorId.ToString(), _hubContext);
                                            var email = TestData.UserList.FirstOrDefault(m => m.UserId == lesson.TutorId).Email;
                                            _senderService.SendMessage(email, Constants.NOTIF_TOMORROW_LESSON);

                                            NotifHub.SendNotification(Constants.NOTIF_TOMORROW_LESSON, lesson.UserId.ToString(), _hubContext);
                                            email = TestData.UserList.FirstOrDefault(m => m.UserId == lesson.UserId).Email;
                                            _senderService.SendMessage(email, Constants.NOTIF_TOMORROW_LESSON);

                                        }
                                    }

                                    if (lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON).NotifValue == false)
                                    {
                                        TimeSpan timeLeft = lessonDate - DateTime.Now; // Время, оставшееся до конца занятия

                                        if (timeLeft.TotalSeconds < 5)
                                        {
                                            TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON).NotifValue = true;
                                            lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON).NotifValue = true;

                                            NotifHub.SendNotification(Constants.NOTIF_START_LESSON, lesson.TutorId.ToString(), _hubContext);
                                            var email = TestData.UserList.FirstOrDefault(m => m.UserId == lesson.TutorId).Email;
                                            _senderService.SendMessage(email, Constants.NOTIF_START_LESSON);


                                            NotifHub.SendNotification(Constants.NOTIF_START_LESSON, lesson.UserId.ToString(), _hubContext);
                                            email = TestData.UserList.FirstOrDefault(m => m.UserId == lesson.UserId).Email;
                                            _senderService.SendMessage(email, Constants.NOTIF_START_LESSON);


                                        }
                                    }

                                }
                                else
                                {
                                    if (lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON).NotifValue == false)
                                    {
                                        DayOfWeek desiredDayOfWeek = lessonDate.DayOfWeek; // День недели занятия
                                        TimeSpan desiredTime = lessonDate.TimeOfDay; // Время начала занятия

                                        DateTime now = DateTime.Now; // Текущее время
                                        DateTime nextDesiredDay = now.AddDays(((int)desiredDayOfWeek - (int)now.DayOfWeek + 7) % 7); // Ближайший день с заданным днем недели
                                        DateTime desiredDateTime = nextDesiredDay.Date + desiredTime; // Дата и время начала занятия

                                        TimeSpan timeLeft = desiredDateTime - now; // Время, оставшееся до занятия

                                        if (timeLeft.TotalHours < 25 && timeLeft.TotalHours > 0) // Если до занятия осталось 1 день или меньше и оно находится в промежутке между 50 минутами и часом
                                        {
                                            TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON).NotifValue = true;
                                            lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON).NotifValue = true;


                                            NotifHub.SendNotification(Constants.NOTIF_TOMORROW_LESSON, lesson.TutorId.ToString(), _hubContext);
                                            var email = TestData.UserList.FirstOrDefault(m => m.UserId == lesson.TutorId).Email;
                                            _senderService.SendMessage(email, Constants.NOTIF_TOMORROW_LESSON);

                                            NotifHub.SendNotification(Constants.NOTIF_TOMORROW_LESSON, lesson.UserId.ToString(), _hubContext);
                                            email = TestData.UserList.FirstOrDefault(m => m.UserId == lesson.UserId).Email;
                                            _senderService.SendMessage(email, Constants.NOTIF_TOMORROW_LESSON);
                                        }

                                    }

                                    if (lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON).NotifValue == false)
                                    {
                                        DayOfWeek desiredDayOfWeek = lessonDate.DayOfWeek; // День недели занятия
                                        TimeSpan desiredTime = lessonDate.TimeOfDay; // Время начала занятия

                                        DateTime now = DateTime.Now; // Текущее время
                                        DateTime nextDesiredDay = now.AddDays(((int)desiredDayOfWeek - (int)now.DayOfWeek + 7) % 7); // Ближайший день с заданным днем недели
                                        DateTime desiredDateTime = nextDesiredDay.Date + desiredTime; // Дата и время начала занятия

                                        TimeSpan timeLeft = desiredDateTime - now; // Время, оставшееся до занятия

                                        if (timeLeft.TotalSeconds < 5)
                                        {
                                            TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON).NotifValue = true;
                                            lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON).NotifValue = true;

                                            NotifHub.SendNotification(Constants.NOTIF_START_LESSON, lesson.TutorId.ToString(), _hubContext);
                                            var email = TestData.UserList.FirstOrDefault(m => m.UserId == lesson.TutorId).Email;
                                            _senderService.SendMessage(email, Constants.NOTIF_START_LESSON);

                                            NotifHub.SendNotification(Constants.NOTIF_START_LESSON, lesson.UserId.ToString(), _hubContext);
                                            email = TestData.UserList.FirstOrDefault(m => m.UserId == lesson.UserId).Email;
                                            _senderService.SendMessage(email, Constants.NOTIF_START_LESSON);
                                        }
                                    }
                                }
                            }
                        }
                        Program.BackInAir = false;

                        Task.Delay(TimeSpan.FromSeconds(40), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Program.BackInAir = false;

                    }
                }

            });

        }
    }
}

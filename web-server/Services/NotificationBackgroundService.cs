using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using web_server.DbContext;

namespace web_server.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IHubContext<NotifHub> _hubContext;

        public NotificationBackgroundService(IHubContext<NotifHub> hubContext)
        {
            _hubContext = hubContext;
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
                        // Получаем все занятия из базы данных
                        var lessons = TestData.Schedules;

                        // Обрабатываем каждое занятие
                        foreach (var lesson in lessons)
                        {
                            var lessonDate = new DateTime(); 

                            if(lesson.StartDate == lessonDate)
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
                                    if (lesson.Tasks[Constatnts.NOTIF_DONT_FORGET_SET_STATUS] == false)
                                    {
                                        TimeSpan timeLeft = DateTime.Now - lessonDate; // время после наачала занятия
                                        if(timeLeft.TotalMinutes > 61)
                                        {
                                            TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks[Constatnts.NOTIF_DONT_FORGET_SET_STATUS] = true;
                                            NotifHub.SendNotification(Constatnts.NOTIF_DONT_FORGET_SET_STATUS, lesson.TutorId.ToString(), _hubContext);
                                        }

                                    }
                                    if (lesson.Tasks[Constatnts.NOTIF_TOMORROW_LESSON] == false)
                                    {
                                        TimeSpan timeLeft = lessonDate - DateTime.Now; // Время, оставшееся до конца занятия

                                        if (timeLeft.TotalHours < 25 && timeLeft.TotalHours > 24)
                                        {
                                            TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks[Constatnts.NOTIF_TOMORROW_LESSON] = true;
                                            NotifHub.SendNotification(Constatnts.NOTIF_TOMORROW_LESSON, lesson.TutorId.ToString(), _hubContext);
                                            NotifHub.SendNotification(Constatnts.NOTIF_TOMORROW_LESSON, lesson.UserId.ToString(), _hubContext);
                                        }
                                    }

                                    if (lesson.Tasks[Constatnts.NOTIF_START_LESSON] == false)
                                    {
                                        TimeSpan timeLeft = lessonDate - DateTime.Now; // Время, оставшееся до конца занятия

                                        if (timeLeft.TotalSeconds < 5)
                                        {
                                            TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks[Constatnts.NOTIF_START_LESSON] = true;
                                            NotifHub.SendNotification(Constatnts.NOTIF_START_LESSON, lesson.TutorId.ToString(), _hubContext);
                                            NotifHub.SendNotification(Constatnts.NOTIF_START_LESSON, lesson.UserId.ToString(), _hubContext);
                                        }
                                    }

                                }
                                else
                                {
                                    if (lesson.Tasks[Constatnts.NOTIF_TOMORROW_LESSON] == false)
                                    {
                                        DayOfWeek desiredDayOfWeek = lessonDate.DayOfWeek; // День недели занятия
                                        TimeSpan desiredTime = lessonDate.TimeOfDay; // Время начала занятия

                                        DateTime now = DateTime.Now; // Текущее время
                                        DateTime nextDesiredDay = now.AddDays(((int)desiredDayOfWeek - (int)now.DayOfWeek + 7) % 7); // Ближайший день с заданным днем недели
                                        DateTime desiredDateTime = nextDesiredDay.Date + desiredTime; // Дата и время начала занятия

                                        TimeSpan timeLeft = desiredDateTime - now; // Время, оставшееся до занятия

                                        if (timeLeft.TotalHours < 25 && timeLeft.TotalHours > 0) // Если до занятия осталось 1 день или меньше и оно находится в промежутке между 50 минутами и часом
                                        {
                                            TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks[Constatnts.NOTIF_TOMORROW_LESSON] = true;
                                            NotifHub.SendNotification(Constatnts.NOTIF_TOMORROW_LESSON, lesson.TutorId.ToString(), _hubContext);
                                            NotifHub.SendNotification(Constatnts.NOTIF_TOMORROW_LESSON, lesson.UserId.ToString(), _hubContext);
                                        }

                                    }

                                    if (lesson.Tasks[Constatnts.NOTIF_START_LESSON] == false)
                                    {
                                        DayOfWeek desiredDayOfWeek = lessonDate.DayOfWeek; // День недели занятия
                                        TimeSpan desiredTime = lessonDate.TimeOfDay; // Время начала занятия

                                        DateTime now = DateTime.Now; // Текущее время
                                        DateTime nextDesiredDay = now.AddDays(((int)desiredDayOfWeek - (int)now.DayOfWeek + 7) % 7); // Ближайший день с заданным днем недели
                                        DateTime desiredDateTime = nextDesiredDay.Date + desiredTime; // Дата и время начала занятия

                                        TimeSpan timeLeft = desiredDateTime - now; // Время, оставшееся до занятия

                                        if (timeLeft.TotalSeconds < 5)
                                        {
                                            TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks[Constatnts.NOTIF_START_LESSON] = true;
                                            NotifHub.SendNotification(Constatnts.NOTIF_START_LESSON, lesson.TutorId.ToString(), _hubContext);
                                            NotifHub.SendNotification(Constatnts.NOTIF_START_LESSON, lesson.UserId.ToString(), _hubContext);
                                        }
                                    }
                                }
                            }
                        }

                         Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
                    }

                    catch (Exception ex)
                    {
                    }
                }

            });
            
        }
    }
}

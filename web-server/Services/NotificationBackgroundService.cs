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
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Получаем все занятия из базы данных
                    var lessons = TestData.Schedules;

                    // Обрабатываем каждое занятие
                    foreach (var lesson in lessons)
                    {
                        // Проверяем статус занятия
                        if (lesson.Status != Models.Status.Проведен)
                        {
                            if (!lesson.Looped)
                            {
                                if (lesson.Tasks[Constatnts.NOTIF_TOMORROW_LESSON] == false)
                                {
                                    TimeSpan timeLeft = lesson.StartDate - DateTime.Now; // Время, оставшееся до конца занятия

                                    if (timeLeft.TotalHours < 25)
                                    {
                                        TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks[Constatnts.NOTIF_TOMORROW_LESSON] = true;
                                        NotifHub.SendNotification(Constatnts.NOTIF_TOMORROW_LESSON, lesson.TutorId.ToString(), _hubContext);
                                        NotifHub.SendNotification(Constatnts.NOTIF_TOMORROW_LESSON, lesson.UserId.ToString(), _hubContext);
                                    }
                                }

                                if (lesson.Tasks[Constatnts.NOTIF_START_LESSON] == false)
                                {
                                    TimeSpan timeLeft = DateTime.Now - lesson.StartDate; // Время, оставшееся до конца занятия

                                    if (timeLeft.TotalSeconds < 20)
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
                                    DayOfWeek desiredDayOfWeek = lesson.StartDate.DayOfWeek; // День недели занятия
                                    TimeSpan desiredTime = lesson.StartDate.TimeOfDay; // Время начала занятия

                                    DateTime now = DateTime.Now; // Текущее время
                                    DateTime nextDesiredDay = now.AddDays(((int)desiredDayOfWeek - (int)now.DayOfWeek + 7) % 7); // Ближайший день с заданным днем недели
                                    DateTime desiredDateTime = nextDesiredDay.Date + desiredTime; // Дата и время начала занятия

                                    TimeSpan timeLeft = desiredDateTime - now; // Время, оставшееся до занятия

                                    if (timeLeft.TotalHours < 25) // Если до занятия осталось 1 день или меньше и оно находится в промежутке между 50 минутами и часом
                                    {
                                        TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks[Constatnts.NOTIF_TOMORROW_LESSON] = true;
                                        NotifHub.SendNotification(Constatnts.NOTIF_TOMORROW_LESSON, lesson.TutorId.ToString(), _hubContext);
                                        NotifHub.SendNotification(Constatnts.NOTIF_TOMORROW_LESSON, lesson.UserId.ToString(), _hubContext);
                                    }

                                }

                                if (lesson.Tasks[Constatnts.NOTIF_START_LESSON] == false)
                                {
                                    DayOfWeek desiredDayOfWeek = lesson.StartDate.DayOfWeek; // День недели занятия
                                    TimeSpan desiredTime = lesson.StartDate.TimeOfDay; // Время начала занятия

                                    DateTime now = DateTime.Now; // Текущее время
                                    DateTime nextDesiredDay = now.AddDays(((int)desiredDayOfWeek - (int)now.DayOfWeek + 7) % 7); // Ближайший день с заданным днем недели
                                    DateTime desiredDateTime = nextDesiredDay.Date + desiredTime; // Дата и время начала занятия

                                    TimeSpan timeLeft = desiredDateTime - now; // Время, оставшееся до занятия

                                    if (timeLeft.TotalSeconds < 20)
                                    {
                                        TestData.Schedules.FirstOrDefault(m => m.Id == lesson.Id).Tasks[Constatnts.NOTIF_START_LESSON] = true;
                                        NotifHub.SendNotification(Constatnts.NOTIF_START_LESSON, lesson.TutorId.ToString(), _hubContext);
                                        NotifHub.SendNotification(Constatnts.NOTIF_START_LESSON, lesson.UserId.ToString(), _hubContext);
                                    }
                                }
                            }
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }

                catch (Exception ex)
                {
                }
            }

        }
    }
}

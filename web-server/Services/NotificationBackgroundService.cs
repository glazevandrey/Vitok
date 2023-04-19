using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading;
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
    public class NotificationBackgroundService : BackgroundService
    {
        //ISenderService _senderService;
        //IScheduleService _scheduleService;
        //ICustomNotificationService await _customNotificationService;
        //UserRepository _userRepository;
        //NotificationRepository _notificationRepository;
        //ScheduleRepository _scheduleRepository;

        private readonly IServiceScopeFactory _scopeFactory;

        public NotificationBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        //public NotificationBackgroundService(ICustomNotificationService customNotificationService, ISenderService senderService, IScheduleService scheduleService)// UserRepository userRepository, NotificationRepository notificationRepository, ScheduleRepository scheduleRepository)
        //{
        //    await _customNotificationService = customNotificationService;
        //    _senderService = senderService;
        //    _scheduleService = scheduleService;
        //    //_userRepository = userRepository;
        //    //_notificationRepository = notificationRepository;
        //    //_scheduleRepository = scheduleRepository;
        //}
        public static DateTime GetNearestScheduleDate(ScheduleDTO cur)
        {
            DateTime date2 = DateTime.UtcNow;
            if (cur.Looped)
            {

                if (cur.ReadyDates.Count > 0)
                {
                    date2 = cur.ReadyDates.Last().Date.AddDays(7);
                }

                if (cur.RescheduledLessons.Count > 0)
                {
                    if (date2 < cur.RescheduledLessons.Last().NewTime)
                    {
                        date2 = cur.RescheduledLessons.Last().NewTime;
                        
                    }
                }
                if (cur.RescheduledDate != DateTime.MinValue)
                {
                    if (date2 < cur.RescheduledDate)
                    {
                        date2 = cur.RescheduledDate;
                       
                    }
                }

                if (cur.SkippedDates.Count > 0)
                {
                    if (date2 < cur.SkippedDates.Last().Date.AddDays(7))
                    {
                        date2 = cur.SkippedDates.Last().Date.AddDays(7);
                       
                    }
                }

                if (date2 < cur.StartDate)
                {
                    date2 = cur.StartDate;
                   

                }
            }
            else
            {
                if (cur.Status == Status.Ожидает)
                {
                    date2 = cur.StartDate;
                    
                }

            }
            return date2;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                Thread.Sleep(10000);
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        Program.BackInAir = true;

                       // List<Schedule> lessons = new List<Schedule>();
                        // Получаем все занятия из базы данных
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            // Разрешаем scoped сервис внутри синглтона
                            var _scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();
                                // Разрешаем scoped сервис внутри синглтона
                            var _senderService = scope.ServiceProvider.GetRequiredService<ISenderService>();
                            var _customNotificationService = scope.ServiceProvider.GetRequiredService<ICustomNotificationService>();

                            var lessons = await _scheduleService.GetAllSchedules();// TestData.Schedules;

                        
                        //var lessons = await _scheduleService.GetAllSchedules();// TestData.Schedules;

                        // Обрабатываем каждое занятие
                        foreach (var lesson in lessons)
                        {

                                if (lesson.UserId == 1)
                            {
                                continue;
                            }
                                var lessonDate = GetNearestScheduleDate(lesson);

                               



                                // Проверяем статус занятия
                            if (lesson.Status != Models.Status.Проведен && lesson.RemoveDate == DateTime.MinValue && lesson.Status != Status.Перенесен && lesson.Status != Status.Пропущен)
                            {
                                if (!lesson.Looped)
                                {
                                    if (lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_DONT_FORGET_SET_STATUS && m.Id != 0).NotifValue == false)
                                    {
                                        TimeSpan timeLeft = DateTime.Now - lessonDate; // время после наачала занятия
                                        if (timeLeft.TotalMinutes > 61)
                                        {
                                            //lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_DONT_FORGET_SET_STATUS).NotifValue = true;
                                            lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_DONT_FORGET_SET_STATUS && m.Id != 0).NotifValue = true;
                                            // var tutor = await _userRepository.GetUserById(lesson.TutorId);

                                            
                                                await _senderService.SendMessage(lesson.TutorId, Constants.NOTIF_DONT_FORGET_SET_STATUS);
                                                await _customNotificationService.SendMessage(Constants.NOTIF_DONT_FORGET_SET_STATUS, lesson.TutorId.ToString());


                                            

                                            //await _customNotificationService.SendMessage(Constants.NOTIF_DONT_FORGET_SET_STATUS, lesson.TutorId.ToString(), _hubContext, _userRepository, _notificationRepository );
                                        }

                                    }
                                    if (lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON && m.Id != 0).NotifValue == false)
                                    {
                                        TimeSpan timeLeft = lessonDate - DateTime.Now; // Время, оставшееся до конца занятия

                                        if (timeLeft.TotalHours < 25 && timeLeft.TotalHours > 23)
                                        {
                                           // lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON).NotifValue = true;
                                            lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON && m.Id != 0).NotifValue = true;



                                            await _customNotificationService.SendMessage(Constants.NOTIF_TOMORROW_LESSON, lesson.TutorId.ToString());
                                            await _senderService.SendMessage(lesson.TutorId, Constants.NOTIF_TOMORROW_LESSON);

                                            await _customNotificationService.SendMessage(Constants.NOTIF_TOMORROW_LESSON, lesson.UserId.ToString());
                                            //var student = await _userRepository.GetUserById(lesson.UserId);
                                            await _senderService.SendMessage(lesson.UserId, Constants.NOTIF_TOMORROW_LESSON);

                                        }
                                    }

                                    if (lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON && m.Id != 0).NotifValue == false)
                                    {
                                        TimeSpan timeLeft = lessonDate - DateTime.Now; // Время, оставшееся до конца занятия

                                        if (timeLeft.TotalSeconds < 5)
                                        {
                                           // lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON).NotifValue = true;
                                            lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON && m.Id != 0).NotifValue = true;

                                            await _customNotificationService.SendMessage(Constants.NOTIF_START_LESSON, lesson.TutorId.ToString());
                                            //var tutor =await _userRepository.GetUserById(lesson.TutorId); //TestData.UserList.FirstOrDefault(m => m.UserId == lesson.TutorId).Email;
                                            await _senderService.SendMessage(lesson.TutorId, Constants.NOTIF_START_LESSON);


                                            await _customNotificationService.SendMessage(Constants.NOTIF_START_LESSON, lesson.UserId.ToString());
                                          //  var student = await _userRepository.GetUserById(lesson.UserId);// TestData.UserList.FirstOrDefault(m => m.UserId == lesson.UserId).Email;
                                            await _senderService.SendMessage(lesson.UserId, Constants.NOTIF_START_LESSON);


                                        }
                                    }

                                }
                                else
                                {
                                    if (lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON && m.Id != 0).NotifValue == false)
                                    {
                                        DayOfWeek desiredDayOfWeek = lessonDate.DayOfWeek; // День недели занятия
                                        TimeSpan desiredTime = lessonDate.TimeOfDay; // Время начала занятия

                                        DateTime now = DateTime.Now; // Текущее время
                                        DateTime nextDesiredDay = now.AddDays(((int)desiredDayOfWeek - (int)now.DayOfWeek + 7) % 7); // Ближайший день с заданным днем недели
                                        DateTime desiredDateTime = nextDesiredDay.Date + desiredTime; // Дата и время начала занятия

                                        TimeSpan timeLeft = desiredDateTime - now; // Время, оставшееся до занятия

                                        if (timeLeft.TotalHours < 25 && timeLeft.TotalHours > 0) // Если до занятия осталось 1 день или меньше и оно находится в промежутке между 50 минутами и часом
                                        {
                                            //lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON).NotifValue = true;
                                            lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON && m.Id != 0).NotifValue = true;


                                            await _customNotificationService.SendMessage(Constants.NOTIF_TOMORROW_LESSON, lesson.TutorId.ToString());
                                           // var tutor = await _userRepository.GetUserById(lesson.TutorId);
                                            await _senderService.SendMessage(lesson.TutorId, Constants.NOTIF_TOMORROW_LESSON);

                                            await _customNotificationService.SendMessage(Constants.NOTIF_TOMORROW_LESSON, lesson.UserId.ToString());
                                            //var student = await _userRepository.GetUserById(lesson.UserId);
                                            await _senderService.SendMessage(lesson.UserId, Constants.NOTIF_TOMORROW_LESSON);
                                        }

                                    }

                                    if (lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON && m.Id != 0).NotifValue == false)
                                    {
                                        DayOfWeek desiredDayOfWeek = lessonDate.DayOfWeek; // День недели занятия
                                        TimeSpan desiredTime = lessonDate.TimeOfDay; // Время начала занятия

                                        DateTime now = DateTime.Now; // Текущее время
                                        DateTime nextDesiredDay = now.AddDays(((int)desiredDayOfWeek - (int)now.DayOfWeek + 7) % 7); // Ближайший день с заданным днем недели
                                        DateTime desiredDateTime = nextDesiredDay.Date + desiredTime; // Дата и время начала занятия

                                        TimeSpan timeLeft = desiredDateTime - now; // Время, оставшееся до занятия

                                        if (timeLeft.TotalSeconds < 5)
                                        {
                                            lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON && m.Id != 0).NotifValue = true;
                                            //await _scheduleService.Update(lesson);

                                            await _customNotificationService.SendMessage(Constants.NOTIF_START_LESSON, lesson.TutorId.ToString());
                                            await _senderService.SendMessage(lesson.TutorId, Constants.NOTIF_START_LESSON);

                                            await _customNotificationService.SendMessage(Constants.NOTIF_START_LESSON, lesson.UserId.ToString());
                                            await _senderService.SendMessage(lesson.UserId, Constants.NOTIF_START_LESSON);
                                        }
                                    }
                                }
                            }

                                await _scheduleService.Update(lesson);
                        }

                        Program.BackInAir = false;
                           await Task.Delay(TimeSpan.FromSeconds(120), stoppingToken);

                        }
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

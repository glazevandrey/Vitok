using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using web_server.Database.Repositories;
using web_server.Models;
using web_server.Models.DTO;
using web_server.Services;
using web_server.Services.Interfaces;

namespace web_server.Quartz
{
    public class QuartzService : IQuartzService
    {
        IHubContext<NotifHub> _hub;
        UserRepository _userRepository;
        NotificationRepository _notificationRepository;
        IMapper _mapper;
        public QuartzService(IHubContext<NotifHub> hub, UserRepository userRepository, NotificationRepository notificationRepository, IMapper mapper)
        {
            _mapper = mapper;
            _hub = hub;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
        }
        public DateTime GetNearestScheduleDate(ScheduleDTO cur)
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

        public async Task MainParse(ISenderService _senderService, IScheduleService _scheduleService)
        {
                Program.BackInAir = true;

                var lessons = await _scheduleService.GetAllSchedules();

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

                                lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_DONT_FORGET_SET_STATUS && m.Id != 0).NotifValue = true;
                                await _scheduleService.Update(lesson);

                                await _senderService.SendMessage(lesson.TutorId, Constants.NOTIF_DONT_FORGET_SET_STATUS);
                                    await NotifHub.SendNotification(Constants.NOTIF_DONT_FORGET_SET_STATUS, lesson.TutorId.ToString(), _hub, _userRepository, _mapper);



                            }

                        }
                            if (lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON && m.Id != 0).NotifValue == false)
                            {
                                TimeSpan timeLeft = lessonDate - DateTime.Now; // Время, оставшееся до конца занятия

                                if (timeLeft.TotalHours < 25 && timeLeft.TotalHours > 23)
                                {

                                    lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_TOMORROW_LESSON && m.Id != 0).NotifValue = true;

                                    await _scheduleService.Update(lesson);
                                    await NotifHub.SendNotification(Constants.NOTIF_TOMORROW_LESSON, lesson.TutorId.ToString(), _hub,_userRepository, _mapper);
                                    await _senderService.SendMessage(lesson.TutorId, Constants.NOTIF_TOMORROW_LESSON);

                                    await NotifHub.SendNotification(Constants.NOTIF_TOMORROW_LESSON, lesson.UserId.ToString(), _hub, _userRepository, _mapper);
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

                                await _scheduleService.Update(lesson);
                                await NotifHub.SendNotification(Constants.NOTIF_START_LESSON, lesson.TutorId.ToString(), _hub, _userRepository, _mapper);
                                    //var tutor =await _userRepository.GetUserById(lesson.TutorId); //TestData.UserList.FirstOrDefault(m => m.UserId == lesson.TutorId).Email;
                                    await _senderService.SendMessage(lesson.TutorId, Constants.NOTIF_START_LESSON);


                                    await NotifHub.SendNotification(Constants.NOTIF_START_LESSON, lesson.UserId.ToString(), _hub, _userRepository, _mapper);
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
                                await _scheduleService.Update(lesson);
                                await NotifHub.SendNotification(Constants.NOTIF_TOMORROW_LESSON, lesson.TutorId.ToString(), _hub, _userRepository, _mapper);
                                    // var tutor = await _userRepository.GetUserById(lesson.TutorId);
                                    await _senderService.SendMessage(lesson.TutorId, Constants.NOTIF_TOMORROW_LESSON);

                                    await NotifHub.SendNotification(Constants.NOTIF_TOMORROW_LESSON, lesson.UserId.ToString(), _hub, _userRepository, _mapper);
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
                                //await _scheduleService.Update(lesson);
                                lesson.Tasks.FirstOrDefault(m => m.NotifKey == Constants.NOTIF_START_LESSON && m.Id != 0).NotifValue = true;
                                await _scheduleService.Update(lesson) ;
                                await NotifHub.SendNotification(Constants.NOTIF_START_LESSON, lesson.TutorId.ToString(), _hub, _userRepository, _mapper);
                                    await _senderService.SendMessage(lesson.TutorId, Constants.NOTIF_START_LESSON);

                                    await NotifHub.SendNotification(Constants.NOTIF_START_LESSON, lesson.UserId.ToString(), _hub, _userRepository, _mapper);
                                    await _senderService.SendMessage(lesson.UserId, Constants.NOTIF_START_LESSON);


                            }
                        }
                        }
                    }

                    await _scheduleService.Update(lesson);
                }

                Program.BackInAir = false;
            }        

    }
}

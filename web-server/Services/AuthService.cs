﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
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
    public class AuthService : IAuthService
    {
        IJsonService _jsonService;
        UserRepository _userRepository;
        ScheduleRepository _scheduleRepository;
        NotificationRepository _notificationRepository;
        IMapper _mapper;
        public AuthService(IJsonService jsonService, IMapper mapper, UserRepository userRepository, ScheduleRepository scheduleRepository, NotificationRepository notificationRepository)
        {
            _mapper = mapper;
            _jsonService = jsonService;
            _userRepository = userRepository;
            _scheduleRepository = scheduleRepository;
            _notificationRepository = notificationRepository;
        }

        public async Task<bool> CheckIsActive(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public async Task<string> GetUserById(string args)
        {

            var split = args.Split(';');
            var id = split[0];
            var role = split[1];

            var user = await _userRepository.GetUserById(Convert.ToInt32(id));

           // var userRole = (await _userRepository.GetUserById(Convert.ToInt32(id))).Role;
            //var userRole = TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(id)).Role;
            string json = "";
            json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(user));
            //if (userRole == "Student")
            //{
            //    student = (Student)await _userRepository.GetUserById(Convert.ToInt32(id));

            //    //student = (Student)TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(id));
            //    json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(student));

            //}
            //else if (userRole == "Tutor")
            //{
            //    tutor = (Tutor)await _userRepository.GetUserById(Convert.ToInt32(id));
            //    json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor));

            //}
            //else
            //{
            //    manager = (Manager)await _userRepository.GetUserById(Convert.ToInt32(id));
            //    json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(manager));
            //}

            return json;
        }

        public async Task<bool> AddRegistration(Registration registration)
        {
            return await _userRepository.AddRegistration(registration);
        }
        public async Task<string> GetUserByToken(string token)
        {
            var user = await _userRepository.GetUserByToken(token);
            if (user == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }
            string json = "";
            try
            {


                json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(user));
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return json;
        }

        public async Task<string> LogIn(string email, string password, HttpContext context)
        {

            TokenProvider _tokenProvider = new TokenProvider(_userRepository);

            var user = await _userRepository.GetUserByEmail(email);

            if (user == null)
                return null;

            var res = _tokenProvider.LoginUser(email, password.Trim(), user);
            if (res == null || res.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Неверное имя пользователя или пароль");

            }
            var userToken = res.First().Key;
            var role = res.First().Value;
            if (userToken != null)
            {
                //var user = await _userRepository.GetUserByEmail(login);
                user.ActiveToken = userToken;
                try
                {
                    await _userRepository.Update(user);
                }
                catch (Exception ex)
                {

                    throw ex;
                }

                context.Response.Cookies.Append(".AspNetCore.Application.Id", userToken,
                    new CookieOptions
                    {
                        MaxAge = TimeSpan.FromMinutes(1160)
                    });

                var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(res));
                return json;
            }

            return _jsonService.PrepareErrorJson("Неверное имя пользователя или пароль");
        }
        public async void DeleteUnPaid(object obj)
        {
            var id = (int)obj;
            var schedule = await _scheduleRepository.GetScheduleById(id);
            //var schedule = TestData.Schedules.FirstOrDefault(m => m.Id == id);
            await _scheduleRepository.RemoveSchedule(schedule);
            //TestData.Schedules.Remove(schedule);
            Program.Timers[id].Dispose();
            Program.Timers.Remove(id);
        }

        public async Task<string> Register(User user, string guid, HttpContext context, IHubContext<NotifHub> _hubContext)
        {
            ((Student)user).StartWaitPayment = DateTime.Now;

            if (await _userRepository.GetUserByEmail(user.Email) != null)
            {
                return null;
            }

            var userid = await _userRepository.AddUser(user);

            Registration reg = null;
            
            reg = await _userRepository.GetRegistrationByGuid(Guid.Parse(guid));
            
            if (reg != null)
            {
                var nearest = reg.WantThis.First().dateTime;

                if (reg.WantThis.Count > 1)
                {

                    foreach (var item in reg.WantThis)
                    {
                        if (item.dateTime < nearest)
                        {
                            nearest = item.dateTime;
                        }
                    }
                }

               // var tutor = await _userRepository.GetUserById(reg.TutorId);
                var datewarn = reg.WantThis.OrderBy(m=>m.dateTime).First().dateTime;
                foreach (var item in reg.WantThis)
                {
                    var tutor = await _userRepository.GetTutor(reg.TutorId);

                    var scheduleToRemove = tutor.Schedules.FirstOrDefault(m => m.StartDate == item.dateTime);
                    if(scheduleToRemove == null)
                    {
                        scheduleToRemove = tutor.Schedules.FirstOrDefault(m => m.StartDate == item.dateTime.AddDays(-7));
                    }

                    var rem = tutor.UserDates.FirstOrDefault(m=>m.dateTime == item.dateTime);
                    if(rem == null)
                    {
                        rem = tutor.UserDates.FirstOrDefault(m=>m.dateTime == item.dateTime.AddDays(-7));
                    }

                    tutor.UserDates.Remove(rem);

                    tutor.Schedules.Remove(scheduleToRemove);

                    if(item.dateTime < DateTime.Now)
                    {
                        while(item.dateTime < DateTime.Now)
                        {
                            item.dateTime = item.dateTime.AddDays(7);
                        }
                    }

                    var sch = new Schedule()
                    {
                        TutorId = reg.TutorId,
                        Course = reg.Course,
                        CourseId = reg.Course.Id,
                        TutorFullName = tutor.FirstName + " " + tutor.LastName,
                        UserId = userid,
                        UserName = user.FirstName + " " + user.LastName,
                        CreatedDate = DateTime.Now,
                        WaitPaymentDate = datewarn == item.dateTime? item.dateTime : DateTime.MinValue,
                        StartDate = item.dateTime,
                        Looped = true,
                        Status = Status.Ожидает
                    };

                    if (nearest == item.dateTime)
                    {
                        sch.WaitPaymentDate = nearest;
                    }
                    await _userRepository.SaveChanges(tutor);

                    var id = await _scheduleRepository.AddSchedule(sch);

                    TimerCallback tm = new TimerCallback(DeleteUnPaid);

                    Program.Timers.Add(id, new System.Threading.Timer(tm, id, 24 * 3600000, 24 * 3600000));
                    var timer = Program.Timers[id];

                    await NotifHub.SendNotification(Constants.NOTIF_NEW_STUDENT_FOR_TUTOR.Replace("{name}", user.FirstName + " " + user.LastName).Replace("{date}", sch.StartDate.ToString("dd.MM.yyyy HH:mm")), reg.TutorId.ToString(), _hubContext, _userRepository, _mapper);

                    await NotifHub.SendNotification(Constants.NOTIF_NEW_STUDENT_FOR_MANAGER.
                        Replace("{studentName}", user.FirstName + " " + user.LastName).
                        Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).
                        Replace("{date}", sch.StartDate.ToString("dd.MM.yyyy HH:mm")),
                        (await _userRepository.GetManagerId()).ToString(), _hubContext, _userRepository, _mapper);


                }


            }

            await LogIn(user.Email, user.Password, context);
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(reg));
        }
    }
}

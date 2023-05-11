using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.IO;
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

            var user = await _userRepository.GetUserById(Convert.ToInt32(id));

            string json = "";
            json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(user));

            return json;
        }

        public async Task<bool> AddRegistration(Registration registration)
        {
            return await _userRepository.AddRegistration(registration);
        }

        public async Task<string> GetLiteUserByToken(string token)
        {
            var user = await _userRepository.GetLiteUserByToken(token);
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

        public async Task<string> GetLiteUserById(string id)
        {
            var user = await _userRepository.GetLiteUser(Convert.ToInt32(id));
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

        public async Task<string> LogIn(string email, string password, HttpContext context, IHubContext<NotifHub> _hubContext)
        {
            return await LogIn(email, password, null, context, _hubContext);
        }
        public async Task<string> LogIn(string email, string password, string guid, HttpContext context, IHubContext<NotifHub> _hubContext)
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


                Registration reg = null;


                if (guid != "00000000-0000-0000-0000-000000000000" && !string.IsNullOrEmpty(guid))
                {
                    reg = await _userRepository.GetRegistrationByGuid(Guid.Parse(guid));

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

                    var tutor = await _userRepository.GetTutor(reg.TutorId);

                    //var datewarn = reg.WantThis.OrderBy(m => m.dateTime).First().dateTime;
                    var managerId = await _userRepository.GetManagerId();

                    foreach (var item in reg.WantThis)
                    {
                        var scheduleToRemove = tutor.Schedules.FirstOrDefault(m => m.StartDate == item.dateTime);
                        if (scheduleToRemove == null)
                        {
                            scheduleToRemove = tutor.Schedules.FirstOrDefault(m => m.StartDate == item.dateTime.AddDays(-7));
                        }

                        var rem = tutor.UserDates.FirstOrDefault(m => m.dateTime == item.dateTime);
                        if (rem == null)
                        {
                            rem = tutor.UserDates.FirstOrDefault(m => m.dateTime == item.dateTime.AddDays(-7));
                        }

                        tutor.UserDates.Remove(rem);

                        tutor.Schedules.Remove(scheduleToRemove);

                        if (item.dateTime < DateTime.Now)
                        {
                            while (item.dateTime < DateTime.Now)
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
                            UserId = user.UserId,
                            UserName = user.FirstName + " " + user.LastName,
                            CreatedDate = DateTime.Now,
                            StartDate = item.dateTime,
                            Looped = true,
                            Status = Status.Ожидает
                        };

                        if (nearest == item.dateTime && ((Student)user).LessonsCount <= 0)
                        {
                            sch.WaitPaymentDate = nearest;
                        }

                        var id = await _scheduleRepository.AddSchedule(sch);


                    }

                    await _userRepository.SaveChanges(tutor);

                    if(user.LessonsCount<= 0)
                    {
                        var stud = await _userRepository.GetStudent(user.UserId);

                        await CalculateNoPaidWarn(stud, _hubContext);
                    }
                    foreach (var item in reg.WantThis)
                    {
                        await NotifHub.SendNotification(Constants.NOTIF_NEW_STUDENT_FOR_TUTOR.Replace("{name}", user.FirstName + " " + user.LastName).Replace("{date}", item.dateTime.ToString("dd.MM.yyyy HH:mm")), reg.TutorId.ToString(), _hubContext, _mapper);

                        await NotifHub.SendNotification(Constants.NOTIF_NEW_STUDENT_FOR_MANAGER.
                            Replace("{studentName}", user.FirstName + " " + user.LastName).
                            Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).
                            Replace("{date}", item.dateTime.ToString("dd.MM.yyyy HH:mm")),
                            (managerId).ToString(), _hubContext, _mapper);

                        await NotifHub.SendNotification(Constants.NOTIF_NEW_LESSON_STUDENT.Replace("{name}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", item.dateTime.ToString("dd.MM.yyyy HH:mm")), user.UserId.ToString(), _hubContext, _mapper);

                    }

                    await _userRepository.RemoveRegistration(reg);


                }

                var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(res));
                return json;
            }



            return _jsonService.PrepareErrorJson("Неверное имя пользователя или пароль");
        }
        public async Task CalculateNoPaidWarn(StudentDTO user, IHubContext<NotifHub> _hubContext)
        {

            if (user.LessonsCount <= 0)
            {
                if (user.StartWaitPayment == DateTime.MinValue || user.StartWaitPayment == null)
                {
                    user.StartWaitPayment = DateTime.Now;
                }
                var ff = user.Schedules.Where(m => m.WaitPaymentDate != DateTime.MinValue).ToList();
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
                var list = user.Schedules.Where(m => m.Status == Status.Ожидает && m.RemoveDate == DateTime.MinValue && m.RemoveDate == DateTime.MinValue).ToList();
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

                var sorted = ScheduleService.SortSchedulesForUnpaid(list);

                foreach (var item in sorted)
                {
                    var sch2 = user.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);
                    //var sch2 = TestData.Schedules.FirstOrDefault(m => m.Id == item.ScheduleId);

                    sch2.WaitPaymentDate = item.Nearest;

                    //await _scheduleRepository.Update(sch2);
                }

                var manager = (await _userRepository.GetManagerId());


                Task.Run(async () =>
                {
                    await NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT, user.UserId.ToString(), _hubContext, _mapper);
                    await NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT_FOR_MANAGER.Replace("{name}",
                        user.FirstName + " " + user.LastName), manager.ToString(), _hubContext, _mapper);
                });

            }

            await _userRepository.SaveChanges(user);

           

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


            if (!string.IsNullOrEmpty(guid))
            {
                reg = await _userRepository.GetRegistrationByGuid(Guid.Parse(guid));
              
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
                var tutor = await _userRepository.GetTutor(reg.TutorId);

                // var tutor = await _userRepository.GetUserById(reg.TutorId);
                var datewarn = reg.WantThis.OrderBy(m => m.dateTime).First().dateTime;
                var managerId = await _userRepository.GetManagerId();
                foreach (var item in reg.WantThis)
                {
                    var scheduleToRemove = tutor.Schedules.FirstOrDefault(m => m.StartDate == item.dateTime);
                    if (scheduleToRemove == null)
                    {
                        scheduleToRemove = tutor.Schedules.FirstOrDefault(m => m.StartDate == item.dateTime.AddDays(-7));
                    }

                    var rem = tutor.UserDates.FirstOrDefault(m => m.dateTime == item.dateTime);
                    if (rem == null)
                    {
                        rem = tutor.UserDates.FirstOrDefault(m => m.dateTime == item.dateTime.AddDays(-7));
                    }

                    tutor.UserDates.Remove(rem);

                    tutor.Schedules.Remove(scheduleToRemove);

                    if (item.dateTime < DateTime.Now)
                    {
                        while (item.dateTime < DateTime.Now)
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
                        WaitPaymentDate = datewarn == item.dateTime ? item.dateTime : DateTime.MinValue,
                        StartDate = item.dateTime,
                        Looped = true,
                        Status = Status.Ожидает
                    };

                    if (nearest == item.dateTime)
                    {
                        sch.WaitPaymentDate = nearest;
                    }

                    var id = await _scheduleRepository.AddSchedule(sch);

                    TimerCallback tm = new TimerCallback(DeleteUnPaid);

                    Program.Timers.Add(id, new System.Threading.Timer(tm, id, 24 * 3600000, 24 * 3600000));
                    var timer = Program.Timers[id];


                }

                await _userRepository.SaveChanges(tutor);

                foreach (var item in reg.WantThis)
                {
                    await NotifHub.SendNotification(Constants.NOTIF_NEW_STUDENT_FOR_TUTOR.Replace("{name}", user.FirstName + " " + user.LastName).Replace("{date}", item.dateTime.ToString("dd.MM.yyyy HH:mm")), reg.TutorId.ToString(), _hubContext, _mapper);

                    await NotifHub.SendNotification(Constants.NOTIF_NEW_LESSON_STUDENT.Replace("{name}", tutor.FirstName + " " + tutor.LastName).Replace("{date}", item.dateTime.ToString("dd.MM.yyyy HH:mm")) + " . Не забудьте пополнить баланс!", userid.ToString(), _hubContext, _mapper);


                    await NotifHub.SendNotification(Constants.NOTIF_NEW_STUDENT_FOR_MANAGER.
                        Replace("{studentName}", user.FirstName + " " + user.LastName).
                        Replace("{tutorName}", tutor.FirstName + " " + tutor.LastName).
                        Replace("{date}", item.dateTime.ToString("dd.MM.yyyy HH:mm")),
                        (managerId).ToString(), _hubContext, _mapper);


                   
                }


            }

            var token = await LogIn(user.Email, user.Password, context, _hubContext);


            await NotifHub.SendNotification(Constants.NOTIF_ZERO_LESSONS_LEFT, user.UserId.ToString(), _hubContext, _mapper);

            if (!string.IsNullOrEmpty(guid))
            {
                return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(reg));
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(token));
        }
    }
}

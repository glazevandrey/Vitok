using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading;
using web_server.DbContext;
using web_server.Models;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class AuthService : IAuthService
    {
        IJsonService _jsonService;
        public AuthService(IJsonService jsonService)
        {
            _jsonService = jsonService;
        }

        public bool CheckIsActive(HttpContext context)
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
        public string GetUserById(string args)
        {

            var split = args.Split(';');
            var id = split[0];
            var role = split[1];
            var user = new User();
            if (role == "Tutor" || role == "Manager")
            {
                user = TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(id));
            }
            else
            {
                user = TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(id));

            }


            var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(user));
            return json;
        }
        public string GetUserByToken(string token)
        {
            var user = TestData.UserList.FirstOrDefault(m => m.ActiveToken == token);
            if (user == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(user));
            return json;
        }

        public string LogIn(User user, HttpContext context)
        {
            TokenProvider _tokenProvider = new TokenProvider();
            var userToken = _tokenProvider.LoginUser(user.Email, user.Password.Trim());
            if (userToken != null)
            {
                TestData.UserList.FirstOrDefault(m => m.Email == user.Email).ActiveToken = userToken;
                context.Response.Cookies.Append(".AspNetCore.Application.Id", userToken,
                    new CookieOptions
                    {
                        MaxAge = TimeSpan.FromMinutes(1160)
                    });

                var json = _jsonService.PrepareSuccessJson(@"""" + userToken + @"""");
                return json;
            }

            return _jsonService.PrepareErrorJson("Неверное имя пользователя или пароль");
        }
        public static void DeleteUnPaid(object obj)
        {
            var id = (int)obj;
            var schedule = TestData.Schedules.FirstOrDefault(m => m.Id == id);
            TestData.Schedules.Remove(schedule);
            Program.Timers[id].Dispose();
            Program.Timers.Remove(id);
        }

        public string Register(User user, HttpContext context, IHubContext<NotifHub> _hubContext)
        {
            user.StartWaitPayment = DateTime.Now;
            TestData.UserList.Add(user);
            var reg = TestData.Registations.FirstOrDefault(m => m.UserId == user.UserId);
            if (reg != null)
            {
                var nearest = reg.WantThis.dateTimes[0];

                if (reg.WantThis.dateTimes.Count > 1)
                {
                    foreach (var item in reg.WantThis.dateTimes)
                    {
                        if (item < nearest)
                        {
                            nearest = item;
                        }
                    }
                }

                foreach (var item in reg.WantThis.dateTimes)
                {
                    var id = TestData.Schedules.Last().Id + 1;

                    var sch = new Schedule()
                    {
                        Id = id,
                        TutorId = reg.TutorId,
                        Course = reg.Course,
                        TutorFullName = TestData.Tutors.FirstOrDefault(m => m.UserId == reg.TutorId).FirstName + " " + TestData.Tutors.FirstOrDefault(m => m.UserId == reg.TutorId).LastName,
                        UserId = reg.UserId,
                        UserName = TestData.UserList.FirstOrDefault(m => m.UserId == reg.UserId).FirstName,
                        Date = new UserDate() { dateTimes = new System.Collections.Generic.List<DateTime>() { item } },
                        CreatedDate = DateTime.Now,
                        StartDate = item,
                        Looped = true,
                        Status = Status.Ожидает
                    };

                    if (nearest == item)
                    {
                        sch.WaitPaymentDate = nearest;
                    }
                    TestData.Schedules.Add(sch);

                    TimerCallback tm = new TimerCallback(DeleteUnPaid);

                    Program.Timers.Add(id, new System.Threading.Timer(tm, id, 24 * 3600000, 24 * 3600000));
                    var timer = Program.Timers[id];

                    NotifHub.SendNotification(Constants.NOTIF_NEW_STUDENT_FOR_TUTOR.Replace("{name}", TestData.UserList.FirstOrDefault(m => m.UserId == reg.UserId).FirstName + " " + TestData.UserList.FirstOrDefault(m => m.UserId == reg.UserId).LastName), reg.TutorId.ToString(), _hubContext);

                    NotifHub.SendNotification(Constants.NOTIF_NEW_STUDENT_FOR_MANAGER.
                        Replace("{studentName}", TestData.UserList.FirstOrDefault(m => m.UserId == reg.UserId).FirstName + " " + TestData.UserList.FirstOrDefault(m => m.UserId == reg.UserId).LastName).
                        Replace("{tutorName}", TestData.UserList.FirstOrDefault(m => m.UserId == reg.TutorId).FirstName + " " + TestData.UserList.FirstOrDefault(m => m.UserId == reg.TutorId).LastName),
                        TestData.Managers.First().UserId.ToString(), _hubContext);

                }

            }

            return LogIn(user, context);
        }
    }
}

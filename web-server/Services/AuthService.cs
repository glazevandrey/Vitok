using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading;
using web_server.DbContext;
using web_server.Models;

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
            if (role == "Tutor")
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
                        MaxAge = TimeSpan.FromMinutes(60)
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

        public string Register(User user, HttpContext context)
        {
            TestData.UserList.Add(user);
            var reg = TestData.Registations.FirstOrDefault(m => m.UserId == user.UserId);
            if (reg != null)
            {

                foreach (var item in reg.WantThis.dateTimes)
                {
                    var id = TestData.Schedules.Last().Id + 1;

                    TestData.Schedules.Add(new Schedule()
                    {
                        Id = id,
                        TutorId = reg.TutorId,
                        Course = reg.Course,
                        TutorFullName = TestData.Tutors.FirstOrDefault(m => m.UserId == reg.TutorId).FirstName + " " + TestData.Tutors.FirstOrDefault(m => m.UserId == reg.TutorId).LastName,
                        UserId = reg.UserId,
                        UserName = TestData.UserList.FirstOrDefault(m => m.UserId == reg.UserId).FirstName,
                        Date = new UserDate() { dateTimes = new System.Collections.Generic.List<DateTime>() { item } },
                        CreatedDate = DateTime.Now,
                        Looped = false,
                        Status = Status.ОжидаетОплату
                    });

                    TimerCallback tm = new TimerCallback(DeleteUnPaid);

                    // TODO: !!!!!!!!!!!!!!! 24 * 3600000!!!!!!!!!!!!!!!!!!!;

                    Program.Timers.Add(id, new System.Threading.Timer(tm, id, 60000, 60000));
                    var timer = Program.Timers[id];
                }

            }

            return LogIn(user, context);
        }
    }
}

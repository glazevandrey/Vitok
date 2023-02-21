using Microsoft.AspNetCore.Http;
using web_server.DbContext;
using web_server.Models;
using System;
using System.Linq;

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
            //if (context.User.Identity.IsAuthenticated)
            //{
            //    return true;
            //}
            return true;
            //return false;

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

            return _jsonService.PrepareErrorJson("Что-то пошло не так. Мы уже работаем над устранением неполадок!");
        }

        public string Register(User user, HttpContext context)
        {
            TestData.UserList.Add(user);
            var reg = TestData.Registations.FirstOrDefault(m => m.UserId == user.UserId);
            if (reg != null)
            {
                TestData.Schedules.Add(new Schedule()
                {
                    Id = 1,
                    TutorId = reg.TutorId,
                    TutorFullName = TestData.Tutors.FirstOrDefault(m => m.UserId == reg.TutorId).FirstName + " " + TestData.Tutors.FirstOrDefault(m => m.UserId == reg.TutorId).LastName,
                    UserId = reg.UserId,
                    UserName = TestData.UserList.FirstOrDefault(m=>m.UserId == reg.UserId).FirstName,
                    Date = reg.WantThis
                }); ;
            }

            return LogIn(user, context);
        }
    }
}

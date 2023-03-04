using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using web_server.DbContext;
using web_server.Models;
using web_server.Services;

namespace web_server.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AccountController : Controller
    {
        IJsonService _jsonService;
        NotificationBackgroundService f;
        private readonly IHubContext<NotifHub> _hubContext;
        public AccountController(IJsonService jsonService, NotificationBackgroundService ff, IHubContext<NotifHub> hubContext)
        {
            f = ff;
            _hubContext = hubContext;
            _jsonService = jsonService;
        }
        [Models.Authorize]
        [HttpGet]
        public string Index()
        {
            return "";
        }

        [Models.Authorize]
        [HttpPost("saveuserinfo", Name = "saveuserinfo")]
        public string SaveUserInfo()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(form.First().Key);
            var old = TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId);

            old.FirstName = user.FirstName;
            old.LastName = user.LastName;
            old.BirthDate = user.BirthDate;
            old.About = user.About;
            old.Email = user.Email;
            old.Wish = user.Wish;
            old.Password = user.Password;
            old.Phone = user.Phone;



            var index = TestData.UserList.FindIndex(m => m.UserId == user.UserId);
            TestData.UserList[index] = old;

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(TestData.UserList[index]));
        }


        [Models.Authorize]
        [HttpPost("addlessons", Name = "addlessons")]
        public string AddLessons()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var args = form.First().Key.Split(";");

            var user = TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(args[0]));
            TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).LessonsCount += Convert.ToInt32(args[1]);
            var scheduled = TestData.Schedules.Where(m => m.UserId == user.UserId).ToList();
            var trial = Convert.ToBoolean(args[2]);
            TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).UsedTrial = true;
            var tariff = TestData.Tariffs.FirstOrDefault(m => m.LessonsCount == Convert.ToInt32(args[1]));
            if (tariff != null)
            {
                TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).BalanceHistory.CustomMessages.Add(DateTime.Now, $"Оплата тарифа: {tariff.Title}");
            }
            else
            {
                TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).BalanceHistory.CustomMessages.Add(DateTime.Now, $"Оплачено занятий: {args[1]}");

            }

            for (int i = 0; i < Convert.ToInt32(args[1]); i++)
            {

                var waited = scheduled.Where(m => m.Status == Status.ОжидаетОплату).ToList();
                if (waited.Count > 0)
                {
                    TestData.Schedules.FirstOrDefault(m => m.Id == waited.First().Id).Status = Status.Ожидает;
                    TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).LessonsCount -= 1;
                }
            }


            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(user));
        }


        [Models.Authorize]
        [HttpGet("getreschedule", Name = "getreschedule")]
        public string GetReSchedule([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка!");
            }

            var user = TestData.UserList.FirstOrDefault(m => m.ActiveToken == args);
            if (user == null)
            {
                return null;
            }

            var schedules = new List<RescheduledLessons>();
            if (user.Role == "Tutor")
            {
                schedules = TestData.RescheduledLessons.Where(m => m.TutorId == user.UserId).ToList();
            }
            else
            {
                schedules = TestData.RescheduledLessons.Where(m => m.UserId == user.UserId).ToList();
            }

            if (schedules == null || schedules.Count == 0)
            {
                schedules = TestData.RescheduledLessons.Where(m => m.TutorId == user.UserId).ToList();

            }
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(schedules));
        }


        [HttpPost("MarkAsRead", Name = "MarkAsRead")]
        public void MarkAsRead([FromForm] int id)
        {
            TestData.Notifications.FirstOrDefault(m => m.Id == Convert.ToInt32(id)).Readed = true;
        }


        [Models.Authorize]
        [HttpGet("getschedule", Name = "getschedule")]
        public string GetSchedule([FromQuery] string args)
        {
            NotifHub.SendNotification("ne lox", "0", _hubContext);
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка!");
            }

            var user = TestData.UserList.FirstOrDefault(m => m.ActiveToken == args);
            if (user == null)
            {
                return null;
            }
            var schedules = TestData.Schedules.Where(m => m.UserId == user.UserId).ToList();
            if (schedules == null || schedules.Count == 0)
            {
                schedules = TestData.Schedules.Where(m => m.TutorId == user.UserId).ToList();

            }
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(schedules));
        }
    }
}

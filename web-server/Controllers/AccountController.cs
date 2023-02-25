using Microsoft.AspNetCore.Mvc;
using web_server.DbContext;
using web_server.Models;
using web_server.Services;
using System.Linq;
using System.Collections.Generic;

namespace web_server.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AccountController : Controller
    {
        IJsonService _jsonService;
        public AccountController(IJsonService jsonService)
        {
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
            var index = TestData.UserList.FindIndex(m => m.UserId == user.UserId);

            if (index == -1 || user == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка!");
            }



            TestData.UserList[index] = user;
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(TestData.UserList[index]));
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

        [Models.Authorize]
        [HttpGet("getschedule", Name = "getschedule")]
        public string GetSchedule([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка!");
            }

            var user = TestData.UserList.FirstOrDefault(m => m.ActiveToken == args);
            if(user == null)
            {
                return null;
            }
            var schedules = TestData.Schedules.Where(m => m.UserId == user.UserId).ToList();
            if(schedules == null || schedules.Count == 0)
            {
                schedules = TestData.Schedules.Where(m => m.TutorId == user.UserId).ToList();

            }
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(schedules));
        }
    }
}

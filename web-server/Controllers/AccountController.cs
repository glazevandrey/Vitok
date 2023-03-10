using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using web_server.DbContext;
using web_server.Services;
using static System.Net.Mime.MediaTypeNames;

namespace web_server.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AccountController : Controller
    {
        IJsonService _jsonService;
        ILessonsService _lessonsService;
        IAccountService _accountService;
        IScheduleService _scheduleService;
        public AccountController(IJsonService jsonService, ILessonsService lessonsService, IAccountService accountService, IScheduleService scheduleService)
        {
            _jsonService = jsonService;
            _lessonsService = lessonsService;
            _accountService = accountService;
            _scheduleService = scheduleService;
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

            var user = _accountService.SaveAccountInfo(form.First().Key);
            if (user != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                return _jsonService.PrepareSuccessJson(json);
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
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

            var user = _lessonsService.AddLessonsToUser(args);
            if (user != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                return _jsonService.PrepareSuccessJson(json);
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }

        [HttpPost("savephoto", Name = "savephoto")]
        public IActionResult SavePhoto([FromQuery]string id)
        {
            var file = Request.Form.Files[0];
            if (file == null)
            {
            }

            var savePath = _accountService.SavePhoto(file, id);


            return Redirect("http://localhost:23571/account");
        }

        [Models.Authorize]
        [HttpGet("getreschedule", Name = "getreschedule")]
        public string GetReSchedule([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка!");
            }

            var list = _lessonsService.GetRescheduledLessons(args);
            if (list != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                return _jsonService.PrepareSuccessJson(json);
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }

        [HttpPost("MarkAsRead", Name = "MarkAsRead")]
        public void MarkAsRead([FromForm] int id) =>
            TestData.Notifications.FirstOrDefault(m => m.Id == Convert.ToInt32(id)).Readed = true;

        [Models.Authorize]
        [HttpGet("getschedule", Name = "getschedule")]
        public string GetSchedule([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка!");
            }

            var list = _scheduleService.GetSchedules(args);
            if (list != null && list.Count > 0)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                return _jsonService.PrepareSuccessJson(json);
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }
    }
}

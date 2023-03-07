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
        ILessonsService _lessonsService;
        IAccountService _accountService;
        private readonly IHubContext<NotifHub> _hubContext;
        public AccountController(IJsonService jsonService, IHubContext<NotifHub> hubContext, ILessonsService lessonsService, IAccountService accountService)
        {
            _hubContext = hubContext;
            _jsonService = jsonService;
            _lessonsService = lessonsService;
            _accountService = accountService;
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
            if(user != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                return _jsonService.PrepareSuccessJson(json);
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }

        [Models.Authorize]
        [HttpGet("getreschedule", Name = "getreschedule")]
        public string GetReSchedule([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка!");
            }

            var list = _accountService.GetRescheduledLessons(args);
            if (list != null && list.Count > 0)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                return _jsonService.PrepareSuccessJson(json);
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }


        [HttpPost("MarkAsRead", Name = "MarkAsRead")]
        public void MarkAsRead([FromForm] int id) => TestData.Notifications.FirstOrDefault(m => m.Id == Convert.ToInt32(id)).Readed = true;


        [Models.Authorize]
        [HttpGet("getschedule", Name = "getschedule")]
        public string GetSchedule([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка!");
            }

            var list = _accountService.GetSchedules(args);
            if (list != null && list.Count > 0)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                return _jsonService.PrepareSuccessJson(json);
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }
    }
}

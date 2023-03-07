﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using web_server.DbContext;
using web_server.Models;
using web_server.Services;

namespace web_server.Controllers
{

    [ApiController]
    [Route("/api/[controller]")]
    public class HomeController : Controller
    {
        private readonly IHubContext<NotifHub> _hubContext;

        IAuthService _authService;
        IJsonService _jsonService;
        ILessonsService _lessonsService;
        public HomeController(IAuthService authService, IHubContext<NotifHub> hub, IJsonService jsonService, ILessonsService lessonsService)
        {
            _hubContext = hub;
            _authService = authService;
            _jsonService = jsonService;
            _lessonsService = lessonsService;
        }

        [HttpPost("loginuser", Name = "loginuser")]
        public string LoginUser()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(form.First().Key);
            var json = _authService.LogIn(user, HttpContext);
            return json;
        }

        [HttpPost("registeruser", Name = "registeruser")]
        public string RegisterUser()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(form.First().Key);
            var json = _authService.Register(user, HttpContext);
            return json;
        }

        [HttpGet("getAllSchedules", Name = "getAllSchedules")]
        public string GetAllSchedules()
        {
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(TestData.Schedules));
        }
        [HttpGet("getAllReSchedules", Name = "getAllReSchedules")]
        public string GetAllReSchedules()
        {
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(TestData.RescheduledLessons));
        }

        [HttpGet("getuser", Name = "getuser")]
        public string GetUser([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var json = _authService.GetUserByToken(args);
            var check = _authService.CheckIsActive(HttpContext);
            if (check == false)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            return json;
        }

        [HttpGet("getAllStudentsAndTutors", Name = "getAllStudentsAndTutors")]
        public string GetAll([FromQuery] string args)
        {
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(TestData.UserList));
        }
        [Authorize]
        [HttpGet("getuserbyid", Name = "getuserbyid")]
        public string GetUserById([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var json = _authService.GetUserById(args);
            var check = _authService.CheckIsActive(HttpContext);
            if (check == false)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");

            }

            return json;
        }
        [Authorize]
        [HttpGet("getschedulebyid", Name = "getschedulebyid")]
        public string GetScheduleById([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var userSchedules = TestData.Schedules.Where(m => m.UserId == Convert.ToInt32(args[0].ToString())).ToList();

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(userSchedules));
        }

        [HttpPost("adduserregistration", Name = "adduserregistration")]
        public string AddUserRegistration()
        {
            var form = Request.Form;
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Registration>(form.First().Key);

            if (form.Keys.Count != 0)
            {
                TestData.Registations.Add(model);
                var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                return json;
            }
            
            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }
        [Authorize]
        [HttpPost("addschedulefromuser", Name = "addschedulefromuser")]
        public string AddScheduleFromUser()
        {
            var form = Request.Form;
            
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Registration>(form.First().Key);
            var user = TestData.UserList.FirstOrDefault(m=>m.UserId == model.UserId);
            if (form.Keys.Count != 0)
            {


                var schedule = TestData.Schedules.FirstOrDefault(m=>m.StartDate.DayOfWeek == model.WantThis.dateTimes[0].DayOfWeek && m.StartDate.ToString("HH:mm") == model.WantThis.dateTimes[0].ToString("HH:mm"));
                schedule.Course = model.Course;
                schedule.UserId= model.UserId;
                schedule.Status = user.LessonsCount == 0 ? Status.ОжидаетОплату : Status.Ожидает;
                schedule.UserName = user.FirstName + " " + user.LastName;
                schedule.CreatedDate = DateTime.Now;

                var text = Constatnts.NOTIF_NEW_LESSON_TUTOR.Replace("{name}", user.FirstName + " " + user.LastName).Replace("{date}", schedule.StartDate.ToString("dd.MM.yyyy HH:mm"));

                // отправка репетитору что у новое занятие
                NotifHub.SendNotification(text, model.TutorId.ToString(), _hubContext);

                var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                return json;
            }


            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }
        [HttpGet("getregistration", Name = "getregistration")]
        public string GetRegistration([FromQuery] string args) =>
                        _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(TestData.Registations.FirstOrDefault(m => m.UserId == Convert.ToInt32(args))));


        [HttpPost("getcontacts", Name = "GetContacts")]
        public string GetContacts() =>
            _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(TestData.Contacts));

    }
}

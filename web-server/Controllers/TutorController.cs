﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using web_server;
using web_server.DbContext;
using web_server.Models;
using web_server.Services.Interfaces;

namespace vitok.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TutorController : Controller
    {

        IJsonService _jsonService;
        ILessonsService _lessonsService;
        ITutorService _tutorService;
        IScheduleService _scheduleService;
        IHubContext<NotifHub> _hubContext;
        public TutorController(IJsonService jsonService, ILessonsService lessonsService, ITutorService tutorService, IScheduleService scheduleService, IHubContext<NotifHub> hubContext)
        {
            _jsonService = jsonService;
            _lessonsService = lessonsService;
            _tutorService = tutorService;
            _scheduleService = scheduleService;
            _hubContext = hubContext;
        }

        [HttpGet("getall", Name = "GetAll")]
        public string GetAllTutors() =>
            _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(TestData.Tutors));


        [HttpGet("gettutor", Name = "gettutor")]
        public string GetTutor([FromQuery] string args) => _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(
                TestData.Tutors.FirstOrDefault(m => m.UserId.ToString() == args)));

        [Authorize]
        [HttpPost("addtutor", Name = "addtutor")]
        public string AddTutor()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;
            var tutor = _tutorService.AddTutor(args);
            if (tutor == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка добавить репетитора");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor));
        }

        [Authorize]
        [HttpPost("updatetutordata", Name = "updatetutordata")]
        public string updatetutordata()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var args = form.First().Key;
            var tutor = _tutorService.UpdateTutor(args);
            if (tutor == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка обновить данные репетитора");
            }


            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor));
        }

        [Authorize]
        [HttpPost("removeTutorServer", Name = "removeTutorServer")]
        public string RemoveTutor()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }
            var args = form.First().Key;

            bool removed = _tutorService.RemoveTutor(args);
            if (!removed)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка удалить репетитора");
            }

            return _jsonService.PrepareSuccessJson("OK");
        }

        [Authorize]
        [HttpPost("addtutorfreedate", Name = "addtutorfreedate")]
        public string AddTutorFreeDate()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;
            var tutor = _tutorService.AddTutorFreeDate(args);
            if (tutor == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка добавить свободные даты");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
        }

        [Authorize]
        [HttpPost("addtutorschedule", Name = "addtutorschedule")]
        public string AddTutorSchedule()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;

            var tutor = _tutorService.AddTutorSchedule(args, _hubContext);
            if (tutor == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка добавить расписание");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
        }

        [Authorize]
        [HttpPost("removetutortimeandschedule", Name = "removetutortimeandschedule")]
        public string RemoveTutorTimeAndSchedule()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;


            var tutor = _tutorService.RemoveTutorSchedule(args, _hubContext);
            if (tutor == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка удалить расписание у репетитора");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
        }

        [Authorize]
        [HttpPost("changeStatusServer", Name = "changeStatusServer")]
        public string ChangeStatus()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }

            var args = form.First().Key;

            var schedule = _scheduleService.ChangeStatus(args, _hubContext);
            if (schedule == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка обновить статус занятия");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(schedule));
        }

        [Authorize]
        [HttpPost("rescheduletutor", Name = "rescheduletutor")]
        public string RescheduleTutor()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;

            var rescheduled = _lessonsService.RescheduleLesson(args, _hubContext);

            if (rescheduled == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка перенести занятие");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(rescheduled));
        }

        [HttpPost("removetutortime", Name = "removetotortime")]
        public string RemoveTutorTime()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;

            var tutor = _tutorService.RemoveTutorTime(args);
            if (tutor == null)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor));
        }
    }
}

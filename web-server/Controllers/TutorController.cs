using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using web_server;
using web_server.Models;
using web_server.Services;
using web_server.Services.Interfaces;

namespace vitok.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TutorController : Controller
    {

        private readonly IJsonService _jsonService;
        private readonly ILessonsService _lessonsService;
        private readonly ITutorService _tutorService;
        private readonly IScheduleService _scheduleService;
        private readonly IAccountService _accountService;
        private readonly IHubContext<NotifHub> _hubContext;

        public TutorController(IAccountService accountService, IJsonService jsonService, ILessonsService lessonsService, ITutorService tutorService, IScheduleService scheduleService, IHubContext<NotifHub> hubContext)
        {
            _accountService = accountService;
            _jsonService = jsonService;
            _lessonsService = lessonsService;
            _tutorService = tutorService;
            _scheduleService = scheduleService;
            _hubContext = hubContext;
        }

        [HttpGet("getall", Name = "GetAll")]
        public async Task<string> GetAllTutors()
        {
            var tutors = await _tutorService.GetAll();
            var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutors));
            return json;
        }


        [HttpGet("gettutor", Name = "gettutor")]
        public async Task<string> GetTutor([FromQuery] string args)
        {
            var tutor = await _tutorService.GetTutor(args);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(tutor);
            return _jsonService.PrepareSuccessJson(json);
        }

        [Authorize]
        [HttpPost("addtutor", Name = "addtutor")]
        public async Task<string> AddTutor()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;
            var tutor = await _tutorService.AddTutor(args);
            if (tutor == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка добавить репетитора");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(true));
        }

        [Authorize]
        [HttpPost("updatetutordata", Name = "updatetutordata")]
        public async Task<string> updatetutordata()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var args = form.First().Key;
            var tutor = await _tutorService.UpdateTutor(args);
            if (tutor == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка обновить данные репетитора");
            }


            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(true));
        }

        [Authorize]
        [HttpPost("removeTutorServer", Name = "removeTutorServer")]
        public async Task<string> RemoveTutor()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }
            var args = form.First().Key;

            bool removed = await _tutorService.RemoveTutor(args);
            if (!removed)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка удалить репетитора");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(true));
        }

        [Authorize]
        [HttpPost("freeTime")]
        public async Task<IActionResult> AddTutorFreeDate([FromBody] AddFreeTimeDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.TutorIdFreeTime))
            {
                return BadRequest(new { success = false, message = "Некорректные данные запроса" });
            }

            try
            {
                // Формируем строку аргументов в старом формате, если _tutorService внутри старого ядра всё еще парсит "id;date;loop"
                bool loop = true;
                string formattedDate = dto.Date2.ToString("dd.MM.yyyy HH:mm");
                string oldArgsFormat = $"{dto.TutorIdFreeTime};{formattedDate};{loop}";

                // Вызываем твой сервис ядра
                var tutor = await _tutorService.AddTutorFreeDate(oldArgsFormat);

                if (tutor == null)
                {
                    return BadRequest(new { success = false, message = "Неудачная попытка добавить свободное время" });
                }

                // Возвращаем успех и обновленные даты
                return Ok(new
                {
                    success = true,
                    message = "Свободное время успешно добавлено",
                    userDates = tutor.UserDates
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Ошибка на сервере: {ex.Message}" });
            }
        }

        [Authorize]
        [HttpPost("rejectStudent", Name = "rejectStudent")]
        public async Task<string> RejectStudent()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key.Split(";");
            var success = await _tutorService.RejectStudent(args, _hubContext);
            if (!success)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка добавить свободные даты");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(true));
        }

        [Authorize]
        [HttpPost("addtutorschedule", Name = "addtutorschedule")]
        public async Task<string> AddTutorSchedule()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;

            var tutor = await _tutorService.AddTutorSchedule(args, _hubContext);
            if (tutor == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка добавить расписание");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
        }

        [Authorize]
        [HttpPost("removetutortimeandschedule", Name = "removetutortimeandschedule")]
        public async Task<string> RemoveTutorTimeAndSchedule()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;


            var tutor = await _tutorService.RemoveTutorSchedule(args, _hubContext);
            if (tutor == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка удалить расписание у репетитора");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
        }

        [Authorize]
        [HttpPost("changeStatusServer", Name = "changeStatusServer")]
        public async Task<string> ChangeStatus()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }

            var args = form.First().Key;

            var result = await _scheduleService.ChangeStatus(args, _hubContext);
            if (result != "OK")
            {
                return _jsonService.PrepareErrorJson(result);
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(result));
        }

        [Authorize]
        [HttpPost("rescheduletutor", Name = "rescheduletutor")]
        public async Task<string> RescheduleTutor()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;

            var rescheduled = await _lessonsService.RescheduleLesson(args, _hubContext);

            if (rescheduled == null)
            {
                return _jsonService.PrepareErrorJson("Выбранная дата занята или некорректна.");
            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(true);
            return _jsonService.PrepareSuccessJson(json);
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetAllTutorStudents([FromQuery] string args)
        {
            var users = await _accountService.GetAllUserContacts(args, "Tutor");

            if (users != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(users);
                return Ok(json);
            }

            return BadRequest("Возникла непредвиденная ошибка");
        }

        public class AddFreeTimeDto
        {
            public string TutorIdFreeTime { get; set; }

            // Передаем дату строкой или DateTime. На фронте будем слать ISO-строку.
            public DateTime Date2 { get; set; }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;
using web_server;
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
        public async Task<string> GetAllTutors() =>
            _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(await _tutorService.GetAll()));


        [HttpGet("gettutor", Name = "gettutor")]
        public async Task<string> GetTutor([FromQuery] string args)
        {
            var tutor = await _tutorService.GetTutor(args);
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor));
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

            return _jsonService.PrepareSuccessJson("OK");
        }

        [Authorize]
        [HttpPost("addtutorfreedate", Name = "addtutorfreedate")]
        public async Task<string> AddTutorFreeDate()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;
            var tutor = await _tutorService.AddTutorFreeDate(args);
            if (tutor == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка добавить свободные даты");
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
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

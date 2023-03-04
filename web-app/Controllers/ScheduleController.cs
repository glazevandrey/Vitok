using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using web_app.Models;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.DbContext;
using web_server.Models;
using web_server.Services;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/schedule")]
    public class ScheduleController : Controller
    {
        IJsonService _jsonService;
        IRequestService _requestService;
        public ScheduleController(IJsonService jsonService, IRequestService requestService)
        {
            _jsonService = jsonService;
            _requestService = requestService;
        }
        public IActionResult Index([FromQuery] string date = null)
        {

            CustomRequestGet req = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res = _requestService.SendGet(req, HttpContext);

            if (!res.success)
            {
                return Redirect("/login");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString());
            ViewData["usertoken"] = user.UserId;

            var result = new ResponseModel();
            if (user.Role == "Manager")
            {
                CustomRequestGet request = new GetAllSchedules();
                result = _requestService.SendGet(request, HttpContext);

                CustomRequestGet req3 = new GetAllTutorsRequest();
                var res3 = _requestService.SendGet(req3, HttpContext);
                ViewData["Tutors"] = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(res3.result.ToString());
            }
            else
            {
                CustomRequestGet request = new GetSchedulesByUserToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
                result = _requestService.SendGet(request, HttpContext);

            }


            if (result == null || !result.success || !res.success)
            {
                return Redirect("/login");
            }

            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(result.result.ToString());

            ViewData["role"] = user.Role;
            ViewData["userid"] = user.UserId;
            ViewData["courses"] = TestData.Courses;

            if (user.Role == "Manager")
            {
                req = new GetAllReSchedules();
                res = _requestService.SendGet(req, HttpContext);
            }
            else
            {
                req = new GetReSchedulesByUserToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
                res = _requestService.SendGet(req, HttpContext);

            }


            var rescheduled = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RescheduledLessons>>(res.result.ToString());
            ViewData["rescheduled"] = rescheduled;

            var modl = new DisplayModelShedule()
            {
                Date = date == null ? DateTime.Now : DateTime.Parse(date),
                Schedules = model
            };

            return View(modl);
        }

        [HttpPost("AddFreeTime", Name = "AddFreeTime")]
        public IActionResult AddFreeTime([FromForm] string date2, [FromForm] string tutorIdFreeTime, [FromForm] string looped)
        {
            var loop = looped == "on" ? true : false;
            var req = new CustomRequestPost("api/tutor/addtutorfreedate", $"{tutorIdFreeTime};{DateTime.Parse(date2).ToString("dd.MM.yyyy HH:mm")};{loop}");
            var res = _requestService.SendPost(req, HttpContext);
            return RedirectToAction("Index", "Schedule");
        }

        [HttpPost("RemoveSchedule", Name = "RemoveSchedule")]
        public IActionResult RemoveSchedule([FromForm] string removeDate, [FromForm] string removeTutorId, [FromForm] string removeUserId, [FromForm] string currRemoveDate)
        {
            var date = DateTime.ParseExact(removeDate, "dd-MM-yyyy-HH-mm", CultureInfo.InvariantCulture);
            var currDate = DateTime.Parse(currRemoveDate);

            CustomRequestPost req = new CustomRequestPost("api/tutor/removetutortimeandschedule", $"{removeTutorId};{removeUserId};{date};{currDate}");
            _requestService.SendPost(req, HttpContext);
            return RedirectToAction("Index", "Schedule");
        }

        [HttpPost("AddSchedule", Name = "AddSchedule")]
        public IActionResult AddTutorSchedule([FromForm] string date3, [FromForm] string tutorIdChoosed, [FromForm] string looped, [FromForm] string userId, [FromForm] string courses)
        {
            var loop = looped == "on" ? true : false;
            var req = new CustomRequestPost("api/tutor/addtutorschedule", $"{tutorIdChoosed};{DateTime.Parse(date3).ToString("dd.MM.yyyy HH:mm")};{loop};{userId};{courses}");
            var res = _requestService.SendPost(req, HttpContext);
            return RedirectToAction("Index", "Schedule");
        }
        [HttpPost("changeStatus", Name = "changeStatus")]
        public IActionResult changeStatus([FromForm] string status, [FromForm] string dateStatus, [FromForm] string userStatus, [FromForm] string tutorStatus, [FromForm] string newDate, [FromForm] string reason, [FromForm] string initiator, [FromForm] string newTime, [FromForm] string looped, [FromForm] string courseId, [FromForm] string currDate)
        {
            bool loop = looped == "on" ? true : false;
            if (status == "Перенесен")
            {
                var req = new CustomRequestPost("api/tutor/rescheduletutor", $"{status};{tutorStatus};" +
                $"{DateTime.Parse(dateStatus).ToString("dd.MM.yyyy HH:mm")};{loop};{userStatus};{newDate};{reason};{initiator};{newTime};{courseId};{currDate}");
                var res = _requestService.SendPost(req, HttpContext);

            }
            else
            {
                var req = new CustomRequestPost("api/tutor/changeStatusServer", $"{status};{tutorStatus};{userStatus};{dateStatus};{currDate}");
                var res = _requestService.SendPost(req, HttpContext);

            }

            return RedirectToAction("Index", "Schedule");
        }
        [HttpGet("UpDate", Name = "UpDate")]
        public IActionResult UpDate(string date)
        {
            var date2 = DateTime.Parse(date).AddDays(7);
            return RedirectToAction("Index", "Schedule", new { date = date2.ToString("dd.MM.yyyy") });
        }
        [HttpGet("DownDate", Name = "DownDate")]
        public IActionResult DownDate(string date)
        {
            var date2 = DateTime.Parse(date).AddDays(-7);
            return RedirectToAction("Index", "Schedule", new { date = date2.ToString("dd.MM.yyyy") });
        }
        [HttpPost]
        public IActionResult AddLesson(string userId, string tutorId, string date)
        {
            return View();
        }
    }
}

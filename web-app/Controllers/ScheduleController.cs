using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using web_app.Models;
using web_app.Requests.Get;
using web_app.Requests;
using web_app.Services;
using web_server.Models;
using web_server.Services.Interfaces;

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
        public IActionResult Index([FromQuery] string date = null, [FromQuery] string error = null)
        {
            CustomRequestGet req = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res = _requestService.SendGet(req, HttpContext);

            if (!res.success)
            {
                if (Request.Cookies.ContainsKey(".AspNetCore.Application.Id"))
                {
                    return Redirect("/account/logout");

                }
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

            if (!res.success)
            {
                return Redirect("/login");
            }

            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(result.result.ToString());

            ViewData["role"] = user.Role;
            ViewData["userid"] = user.UserId;
            ViewData["courses"] = user.Courses != null ? user.Courses : new List<Course>();
            ViewData["lessons"] = user.LessonsCount;
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;



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


            CustomRequestGet request2 = new GetAllUsersRequest();
            var result2 = _requestService.SendGet(request2, HttpContext);
            var users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(result2.result.ToString());
            Dictionary<int, DateTime> keyValuePairs = new Dictionary<int, DateTime>();
            foreach (var item in users)
            {
                if (item.StartWaitPayment != DateTime.MinValue)
                {
                    keyValuePairs.Add(item.UserId, item.StartWaitPayment);
                }
            }

            if (user.Role == "Student")
            {
                ViewData["firstPay"] = user.WasFirstPayment;
                if (user.FirstLogin == true)
                {
                    ViewData["firstLogin"] = true;
                }
            }
            else
            {
                var dic = new Dictionary<int, bool>();
                foreach (var item in users)
                {
                    if (!dic.ContainsKey(item.UserId))
                    {
                        dic.Add(item.UserId, item.WasFirstPayment);
                    }
                }
                ViewData["firstPay"] = dic;
            }


            ViewData["waited"] = keyValuePairs;
            var modl = new DisplayModelShedule()
            {
                Date = date == null ? DateTime.Now : DateTime.Parse(date),
                Schedules = model
            };

            if (error != null)
            {
                ViewData["error"] = error;
            }
            return View(modl);
        }

        [HttpPost("AddFreeTime", Name = "AddFreeTime")]
        public IActionResult AddFreeTime([FromForm] string date2, [FromForm] string tutorIdFreeTime)
        {
            bool loop = true;
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

            var date1 = DateTime.Parse(date3);
            var date2 = DateTime.Now;
            if (date1 <= date2)
            {
                return RedirectToAction("Index", "Schedule", new { error = "Нельзя создать занятие на дату которая меньше текущей." });
            }

            var loop = looped == "on" ? true : false;
            var req = new CustomRequestPost("api/tutor/addtutorschedule", $"{tutorIdChoosed};{DateTime.Parse(date3).ToString("dd.MM.yyyy HH:mm")};{loop};{userId};{courses}");
            var res = _requestService.SendPost(req, HttpContext);
            if (!res.success)
            {
                return RedirectToAction("Index", "Schedule", new { error = res.result.ToString() });
            }
            return RedirectToAction("Index", "Schedule");
        }
        [HttpPost("changeStatus", Name = "changeStatus")]
        public IActionResult changeStatus([FromForm] string status, [FromForm] string userMakeWarn, [FromForm] string dateStatus, [FromForm] string userStatus, [FromForm] string tutorStatus, [FromForm] string newDate, [FromForm] string reason, [FromForm] string initiator, [FromForm] string newTime, [FromForm] string looped, [FromForm] string courseId, [FromForm] string currDate)
        {
            bool loop = looped == "on" ? true : false;
            bool warn = userMakeWarn == "on" ? true : false;
            if (status == "Перенесен")
            {
                var date1 = DateTime.Parse(newDate + " " + newTime + ":00");
                var date2 = DateTime.Now;
                if (date1 <= date2)
                {
                    return RedirectToAction("Index", "Schedule", new { error = "Нельзя перенести занятие на дату которая меньше текущей." });
                }

                var req = new CustomRequestPost("api/tutor/rescheduletutor", $"{status};{tutorStatus};" +
                $"{DateTime.Parse(dateStatus).ToString("dd.MM.yyyy HH:mm")};{loop};{userStatus};{newDate};{reason};{initiator};{newTime + ":00"};{courseId};{currDate}");
                var res = _requestService.SendPost(req, HttpContext);
                if (!res.success)
                {
                    return RedirectToAction("Index", "Schedule", new { error = res.result.ToString() });
                }

            }
            else
            {
                // TODO: REMOVE AND
                if (DateTime.Parse(currDate) > DateTime.Now.AddMinutes(55) && tutorStatus != 1.ToString())
                {
                    return RedirectToAction("Index", "Schedule", new { error = "Нельзя провести занятие если его дата меньше текущей с учетом времени занятия." });
                }

                var req = new CustomRequestPost("api/tutor/changeStatusServer", $"{status};{tutorStatus};{userStatus};{dateStatus};{currDate};{warn}");
                var res = _requestService.SendPost(req, HttpContext);
                if (!res.success)
                {
                    return RedirectToAction("Index", "Schedule", new { error = res.result.ToString() });
                }

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
        [HttpGet("SetDate", Name = "SetDate")]
        public IActionResult SetDate(string date)
        {
            var date2 = DateTime.Parse(date);
            return RedirectToAction("Index", "Schedule", new { date = date2.ToString("dd.MM.yyyy") });
        }
        [HttpPost]
        public IActionResult AddLesson(string userId, string tutorId, string date)
        {
            return View();
        }
    }
}

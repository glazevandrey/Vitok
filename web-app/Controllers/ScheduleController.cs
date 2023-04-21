using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using web_app.Requests;
using web_app.Requests.Get;
using web_app.Services;
using web_server.Models;
using web_server.Models.DBModels;
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

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString(), Program.settings);
            ViewData["usertoken"] = user.UserId;

            ViewData["role"] = user.Role;
            ViewData["userid"] = user.UserId;
            var model = new List<Schedule>();

            if (user.Role == "Tutor")
            {
                ViewData["courses"] = ((Tutor)user).Courses != null ? ((Tutor)user).Courses : new List<Course>();
                model = ((Tutor)user).Schedules;

            }
            else
            {

                var _req = new GetCourses();
                var _res = _requestService.SendGet(_req);
                var _list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Course>>(_res.result.ToString());
                ViewData["courses"] = _list;
            }

            if (user.Role == "Student")
            {
                ViewData["lessons"] = ((Student)user).LessonsCount;
                model = ((Student)user).Schedules;
            }
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;



            if (user.Role == "Manager")
            {
                return Redirect("/manageschool");

            }


            CustomRequestGet request2 = new GetAllUsersRequest(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var result2 = _requestService.SendGet(request2, HttpContext);
            if (!result2.success)
            {
                return Redirect("/account/logout");
            }
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new UserConverter());
            var users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(result2.result.ToString(), settings);
            Dictionary<int, DateTime> keyValuePairs = new Dictionary<int, DateTime>();
            foreach (var item in users.Where(m => m.Role == "Student"))
            {
                if (((Student)item).StartWaitPayment != DateTime.MinValue)
                {
                    keyValuePairs.Add(item.UserId, ((Student)item).StartWaitPayment);
                }
            }

            if (user.Role == "Student")
            {
                ViewData["firstPay"] = ((Student)user).WasFirstPayment;
                ViewData["firstLogin"] = ((Student)user).FirstLogin;

            }
            else
            {
                var dic = new Dictionary<int, bool>();
                foreach (var item in users.Where(m => m.Role == "Student").ToList())
                {
                    if (!dic.ContainsKey(item.UserId))
                    {
                        dic.Add(item.UserId, ((Student)item).WasFirstPayment);
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
            var ff = model.FirstOrDefault(m => m.StartDate == DateTime.Parse("21.04.2023 10:00"));
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
            if (date1 <= date2 && tutorIdChoosed != 4.ToString())
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
                if (DateTime.Parse(currDate) > DateTime.Now.AddMinutes(55) && tutorStatus != 4.ToString())
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

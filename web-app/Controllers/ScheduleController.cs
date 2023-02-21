using Microsoft.AspNetCore.Mvc;
using web_server.Models;
using web_server.Services;
using System.Collections.Generic;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using System;
using System.Globalization;

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
            CustomRequestGet request = new GetSchedulesByUserToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var result = _requestService.SendGet(request, HttpContext);

            CustomRequestGet req = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res = _requestService.SendGet(req, HttpContext);
            if (res == null || result == null || !result.success || !res.success )
            {
                return Redirect("/login");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString());

            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(result.result.ToString());
            ViewData["role"] = user.Role;
            ViewData["userd"] = user.UserId;
            var modl = new DisplayModelShedule()
            {
                Date = date == null? DateTime.Now : DateTime.Parse(date),
                Schedules = model
            };
            return View(modl);
        }
        [HttpPost]
        public IActionResult AddFreeTime(string date, string tutorId)
        {
            return View();
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

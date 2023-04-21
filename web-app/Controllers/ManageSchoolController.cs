using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using web_app.Requests;
using web_app.Requests.Get;
using web_app.Services;
using web_server.Models;
using web_server.Models.DBModels;

namespace web_app.Controllers
{
    [ApiController]
    [Route("manageschool")]
    public class ManageSchoolController : Controller
    {
        IRequestService _requestService;
        public ManageSchoolController(IRequestService requestService)
        {
            _requestService = requestService;
        }
        public IActionResult Index([FromQuery] string date = null)
        {

            CustomRequestGet request = new GetUserByToken(Request.Cookies[".AspNetCore.Application.Id"]);
            var result = _requestService.SendGet(request, HttpContext);

            if (result.success == false)
            {
                return Redirect("/login");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<Manager>(result.result.ToString());


            result = new ResponseModel();
            request = new GetAllSchedules();
            result = _requestService.SendGet(request, HttpContext);

            CustomRequestGet req3 = new GetAllTutorsRequest();
            var res3 = _requestService.SendGet(req3, HttpContext);
            ViewData["Tutors"] = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tutor>>(res3.result.ToString());


            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(result.result.ToString());


            //request = new GetAllReSchedules();
            //result = _requestService.SendGet(request, HttpContext);

            //var rescheduled = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RescheduledLessons>>(result.result.ToString());
            //ViewData["rescheduled"] = rescheduled;

            ViewData["userid"] = user.UserId;
            ViewData["role"] = user.Role;
            ViewData["lessons"] = user.LessonsCount;
            ViewData["usertoken"] = user.UserId;
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;


            CustomRequestGet request2 = new GetAllUsersRequest(Request.Cookies[".AspNetCore.Application.Id"]);
            var result2 = _requestService.SendGet(request2, HttpContext);

            var users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(result2.result.ToString(), Program.settings);
            Dictionary<int, DateTime> keyValuePairs = new Dictionary<int, DateTime>();
            foreach (var item in users)
            {
                if (item.StartWaitPayment != DateTime.MinValue)
                {
                    keyValuePairs.Add(item.UserId, item.StartWaitPayment);
                }
            }
            ViewData["waited"] = keyValuePairs;


            var dic = new Dictionary<int, bool>();
            foreach (var item in users)
            {
                if (!dic.ContainsKey(item.UserId))
                {
                    dic.Add(item.UserId, item.WasFirstPayment);
                }
            }
            ViewData["firstPay"] = dic;

            var modl = new DisplayModelShedule()
            {
                Date = date == null ? DateTime.Now : DateTime.Parse(date),
                Schedules = model
            };

            return View(modl);
        }
        [HttpGet("UpDateM", Name = "UpDateM")]
        public IActionResult UpDateM(string date)
        {
            var date2 = DateTime.Parse(date).AddDays(1);
            return RedirectToAction("Index", "ManageSchool", new { date = date2.ToString("dd.MM.yyyy") });
        }
        [HttpGet("DownDateM", Name = "DownDateM")]
        public IActionResult DownDateM(string date)
        {
            var date2 = DateTime.Parse(date).AddDays(-1);
            return RedirectToAction("Index", "ManageSchool", new { date = date2.ToString("dd.MM.yyyy") });
        }
        [HttpGet("SetDateM", Name = "SetDateM")]
        public IActionResult SetDateM(string setdate)
        {
            var date2 = DateTime.Parse(setdate);
            return RedirectToAction("Index", "ManageSchool", new { date = date2.ToString("dd.MM.yyyy") });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using web_app.Models;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.DbContext;
using web_server.Models;
using web_server.Services.Interfaces;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/students")]
    public class StudentsController : Controller
    {
        IJsonService _jsonService;
        IRequestService _requestService;
        public StudentsController(IJsonService jsonService, IRequestService requestService)
        {
            _jsonService = jsonService;
            _requestService = requestService;
        }
        public IActionResult Index()
        {
            CustomRequestGet req4 = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res4 = _requestService.SendGet(req4, HttpContext);

            if (!res4.success)
            {
                if (!string.IsNullOrEmpty(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]))
                {
                    return Redirect("/login");

                }
                else
                {
                    return Redirect("/logout");

                }
            }

            var currUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res4.result.ToString());
            
            ViewData["usertoken"] = currUser.UserId;
            ViewData["photoUrl"] = currUser.PhotoUrl;
            ViewData["displayName"] = currUser.FirstName + " " + currUser.LastName;

            var req = new GetAllUsersRequest();
            var res = _requestService.SendGet(req, HttpContext);
            if (!res.success)
            {
                return Redirect("/login");
            }
            ViewData["role"] = "Manager";
            var users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(res.result.ToString());
            users = users.Where(m => m.Role == "Student").ToList();
            var schedules = new List<Schedule>();

            foreach (var user in users)
            {
                var req2 = new GetSchedulesByUserId(user.UserId.ToString());
                res = _requestService.SendGet(req2, HttpContext);

                if (res.success)
                {
                    schedules.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(res.result.ToString()));
                }

            }

            ViewData["schedules"] = schedules;
            return View(users);
        }


        [HttpGet("statistics", Name = "statistics")]
        public IActionResult Statistics([FromQuery] string userid)
        {
            CustomRequestGet req = new GetUserById(userid + ";" + "Manager");
            var res = _requestService.SendGet(req, HttpContext);
            if (!res.success)
            {
                return Redirect("/login");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString());

            ViewData["role"] = user.Role;
            ViewData["tariffs"] = TestData.Tariffs;
            ViewData["usertoken"] = user.UserId;
            ViewData["lessons"] = user.LessonsCount;
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;
            if (user.FirstLogin == true && user.Role == "Student")
            {
                ViewData["firstLogin"] = true;
            }


            var req2 = new GetStatisticsData(user.UserId.ToString());
            var res2 = _requestService.SendGet(req2, HttpContext);
            if (!res2.success)
            {
                return View(new Dictionary<DateTime, List<StudentPayment>>());
            }

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<DateTime, List<StudentPayment>>>(res2.result.ToString());
            return View(data);
        }

      
        [HttpGet("info", Name = "info")]
        public IActionResult Info([FromQuery] string id)
        {
            CustomRequestGet req4 = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res4 = _requestService.SendGet(req4, HttpContext);

            if (!res4.success)
            {
                return Redirect("/login");
            }
            var currUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res4.result.ToString());
            ViewData["usertoken"] = currUser.UserId;
            ViewData["photoUrl"] = currUser.PhotoUrl;
            ViewData["role"] = "Manager";




            var req = new GetUserById(id + ";Manager");
            var data = _requestService.SendGet(req, HttpContext);
            if (!data.success)
            {
                return Redirect("/login");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(data.result.ToString());

            return View(user);
        }
    }
}

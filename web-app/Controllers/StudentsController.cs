using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using web_app.Requests;
using web_app.Requests.Get;
using web_app.Services;
using web_server.Models.DBModels;
using web_server.Services.Interfaces;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/students")]
    public class StudentsController : Controller
    {
        IJsonService _jsonService;
        private readonly IWebHostEnvironment _env;

        IRequestService _requestService;
        public StudentsController(IJsonService jsonService, IRequestService requestService, IWebHostEnvironment env)
        {
            _jsonService = jsonService;
            _requestService = requestService;
            _env = env;
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

            var currUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res4.result.ToString(), Program.settings);

            ViewData["usertoken"] = currUser.UserId;
            ViewData["photoUrl"] = currUser.PhotoUrl;
            ViewData["displayName"] = currUser.FirstName + " " + currUser.LastName;

            var req = new GetAllUsersRequest(Request.Cookies[".AspNetCore.Application.Id"]);
            var res = _requestService.SendGet(req, HttpContext);
            if (!res.success)
            {
                return Redirect("/login");
            }
            ViewData["role"] = "Manager";
            var users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Student>>(res.result.ToString(), Program.settings);
            users = users.Where(m => m.Role == "Student").Distinct().ToList();
            var schedules = new List<Schedule>();

            foreach (var user in users)
            {

                if (res.success)
                {
                    schedules.AddRange(user.Schedules);
                }

            }

            ViewData["schedules"] = schedules;
            return View(users);
        }


        [HttpGet("statistics", Name = "statistics")]
        public IActionResult Statistics([FromQuery] string userid, [FromQuery] string username)
        {
            Stopwatch ss = Stopwatch.StartNew();
            CustomRequestGet req4 = new GetLiteUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
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

            var currUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res4.result.ToString(), Program.settings);


            ViewData["role"] = currUser.Role;
            ViewData["usertoken"] = currUser.UserId;

            ViewData["photoUrl"] = currUser.PhotoUrl;
            ViewData["displayName"] = currUser.FirstName + " " + currUser.LastName;

          
            var req2 = new GetStatisticsData(userid);
            var res2 = _requestService.SendGet(req2, HttpContext);
            if (!res2.success)
            {
                return View(new Dictionary<DateTime, List<StudentPayment>>());
            }

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<DateTime, List<StudentPayment>>>(res2.result.ToString());
            ViewData["statuser"] = username.Split("_")[0] + " " + username.Split("_")[1];
            ss.Stop();
            var time = ss.Elapsed;
            Program.requestTimes.Add(new RequestTime() { time = time, url = "STAT TEST" });
            return View(data);
        }


        [HttpGet("info", Name = "info")]
        public IActionResult Info([FromQuery] string id)
        {
            CustomRequestGet req4 = new GetLiteUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res4 = _requestService.SendGet(req4, HttpContext);

            if (!res4.success)
            {
                return Redirect("/login");
            }
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new UserConverter());

            var currUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res4.result.ToString(), settings);
            ViewData["usertoken"] = currUser.UserId;
            ViewData["photoUrl"] = currUser.PhotoUrl;
            ViewData["role"] = "Manager";




            var req = new GetLiteUserById(id);
            var data = _requestService.SendGet(req, HttpContext);
            if (!data.success)
            {
                return Redirect("/login");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<Student>(data.result.ToString(), Program.settings);
            
            
            return View(user);
        }
    }
}

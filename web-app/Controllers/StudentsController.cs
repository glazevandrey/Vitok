using CsvHelper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using web_app.Requests.Get;
using web_app.Requests;
using web_app.Services;
using web_server.DbContext;
using web_server.Models;
using web_server.Services.Interfaces;
using web_server.Models.DBModels;

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
            users = users.Where(m => m.Role == "Student").ToList();
            var schedules = new List<Schedule>();

            foreach (var user in users)
            {
                //var req2 = new GetSchedulesByUserId(user.UserId.ToString());
                //res = _requestService.SendGet(req2, HttpContext);

                if (res.success)
                {
                    schedules.AddRange(user.Schedules);
                    //  schedules.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(res.result.ToString()));
                }

            }

            ViewData["schedules"] = schedules;
            return View(users);
        }


        [HttpGet("statistics", Name = "statistics")]
        public IActionResult Statistics([FromQuery] string userid)
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


            CustomRequestGet req = new GetUserById(userid + ";" + "Manager");
            var res = _requestService.SendGet(req, HttpContext);
            if (!res.success)
            {
                return Redirect("/login");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString(), Program.settings);


            var req3 = new GetTariffsRequest();
            var res3 = _requestService.SendGet(req3);
            var tariffs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tariff>>(res3.result.ToString());

            ViewData["role"] = currUser.Role;
            ViewData["tariffs"] = tariffs;
            ViewData["usertoken"] = currUser.UserId;

            if (user.Role == "Student")
            {
                ViewData["lessons"] = ((Student)user).LessonsCount;
            }
            ViewData["photoUrl"] = currUser.PhotoUrl;
            ViewData["displayName"] = currUser.FirstName + " " + currUser.LastName;

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
            ViewData["statuser"] = user.FirstName + " " + user.LastName;
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

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(data.result.ToString(), Program.settings);

            return View(user);
        }
    }
}

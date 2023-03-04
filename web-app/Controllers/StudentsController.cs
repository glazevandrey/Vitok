﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.Models;
using web_server.Services;

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
                return Redirect("/login");
            }
            var currUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res4.result.ToString());
            ViewData["usertoken"] = currUser.UserId;

            var req = new GetAllUsersRequest();
            var res = _requestService.SendGet(req, HttpContext);
            if (!res.success)
            {
                return Redirect("/login");
            }
            ViewData["role"] = "Manager";
            var users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(res.result.ToString());
            users = users.Where(m=>m.Role == "Student").ToList();
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
    }
}

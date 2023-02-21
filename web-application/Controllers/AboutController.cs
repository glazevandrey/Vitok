﻿using Microsoft.AspNetCore.Mvc;
using server.DbContext;
using server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using web_application.Models.Requests;
using web_application.Models.Requests.Get;
using web_application.Services;

namespace web_application.Controllers
{
    [ApiController]
    [Route("/about")]
    public class AboutController : Controller
    {
        IRequestService _requestService;
        public AboutController(IRequestService requestService)
        {
            _requestService = requestService;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("tutors", Name = "tutors")]
        public IActionResult Register()
        {
            CustomRequestGet req = new GetAllTutorsRequest();
            var response = _requestService.SendGet(req, HttpContext);
            if (response == null)
            {
                return BadRequest("Технические проблемы. Мы уже исправляем!");
            }

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(response.result.ToString());
            return View(data);
        }

        [HttpPost("fromregistertologin", Name = "fromregistertologin")]
        public IActionResult FromRegisterToLogin()
        {
            var form = Request.Form;
            var date = new UserDate();
            int tutorId = 0;
            if (form.Count != 0)
            {
                tutorId = Convert.ToInt32(form["tutor"]);

                CustomRequestGet req = new GetTutorByIdRequest(tutorId.ToString());
                var response = _requestService.SendGet(req, HttpContext);
                if (response.success == false)
                {
                    return BadRequest("Что-то пошло не так =(");
                }
                var time = form["texdt"];
                var times = time.ToString().Split(',');

                foreach (var item in times)
                {
                    date.dateTimes.Add(DateTime.Parse(item));
                }
            }

            Registration registration = new Registration
            {
                UserId = TestData.UserList.Last().UserId+1,
                WantThis = date,
                TutorId = tutorId
            };

            CustomRequestPost requestPost = new CustomRequestPost("api/home/AddUserRegistration", data: registration);
            var res = _requestService.SendPost(requestPost, null);
            if (res.success == false)
            {
                return BadRequest("Что-то пошло не так =(");
            }

            return Redirect($"/login?id={registration.UserId}");
        }

        [HttpPost("details", Name = "details")]
        public IActionResult Details([FromQuery] string id)
        {
            CustomRequestGet req = new GetTutorByIdRequest(id);
            var response = _requestService.SendGet(req, HttpContext);
            if (response == null)
            {
                return BadRequest("Технические проблемы. Мы уже исправляем!");
            }
            var tutor = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(response.result.ToString());
            return View(tutor);
        }

    }
}

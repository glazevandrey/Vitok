﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using web_app.Requests.Get;
using web_app.Requests;
using web_app.Services;
using web_server.DbContext;
using web_server.Services.Interfaces;
using web_server.Models.DBModels;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/tutors")]
    public class TutorsController : Controller
    {
        IJsonService _jsonService;
        IRequestService _requestService;

        public TutorsController(IJsonService jsonService, IRequestService requestService)
        {
            _jsonService = jsonService;
            _requestService = requestService;
        }

        public IActionResult Index()
        {
            CustomRequestGet req = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);

            var res = _requestService.SendGet(req, HttpContext);
            if (!res.success)
            {
                return Redirect("/login");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString(), Program.settings);
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;
            ViewData["usertoken"] = user.UserId;

            var req2 = new GetAllUsersRequest(Request.Cookies[".AspNetCore.Application.Id"]);

            var res2 = _requestService.SendGet(req2, HttpContext);
            if (!res2.success)
            {
                return Redirect("/login");
            }

            ViewData["role"] = "Manager";

            var users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tutor>>(res2.result.ToString(), Program.settings);
            users = users.Where(m => m.Role == "Tutor").ToList();

            return View(users);
        }
        [HttpPost("removeTutor", Name = "removeTutor")]
        public IActionResult RemoveTutor([FromForm] string userIdRemove)
        {
            var req1 = new GetUserById(userIdRemove + ";Tutor");
            var res1 = _requestService.SendGet(req1, HttpContext);

            if (!res1.success)
            {
                return Redirect("/login");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res1.result.ToString(), Program.settings);

            var req = new CustomRequestPost("api/tutor/removeTutorServer", user);
            var res = _requestService.SendPost(req, HttpContext);
            if (!res.success)
            {
                return Redirect("/login");
            }
            return RedirectToAction("Index", "Tutors");
        }
        [HttpPost("updateTutor", Name = "updateTutor")]
        public IActionResult UpdateTutor([FromForm] string userIdEdit, [FromForm] string firstNameEdit, [FromForm] string lastNameEdit, [FromForm] string middleNameEdit, [FromForm] string birthDateEdit,
        [FromForm] string emailEdit, [FromForm] string passwordEdit, [FromForm] string phoneEdit, [FromForm] string coursesEdit)
        {
            var course = coursesEdit?.Trim().Trim().Split(";");

            var listCourses = new List<Course>();

            var req3 = new GetCourses();
            var res3 = _requestService.SendGet(req3);

            var courses = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Course>>(res3.result.ToString()); 
            if (course != null)
            {
                foreach (var item in course)
                {
                    var c = courses.FirstOrDefault(m => m.Title == item);
                    if (c != null)
                    {
                        listCourses.Add(c);
                    }
                }

            }

            var user = new Tutor()
            {
                FirstName = firstNameEdit,
                LastName = lastNameEdit,
                UserId = Convert.ToInt32(userIdEdit),
                Phone = phoneEdit,
                Password = passwordEdit,
                BirthDate = DateTime.Parse(birthDateEdit),
                Email = emailEdit,
                MiddleName = middleNameEdit,
                Courses = listCourses,
            };

            CustomRequestPost req = new CustomRequestPost("api/tutor/updatetutordata", user);
            var response = _requestService.SendPost(req, HttpContext);
            if (response == null)
            {

                return BadRequest("Неудачная попытка сохранить данные");
            }
            if (!response.success)
            {
                return Redirect("/login");
            }

            return RedirectToAction("Index", "Tutors");
        }

        [HttpPost("addnewtutor", Name = "addnewtutor")]
        public IActionResult AddNewTutor([FromForm] string firstName, [FromForm] string lastName, [FromForm] string middleName, [FromForm] string birthDate,
            [FromForm] string email, [FromForm] string password, [FromForm] string phone, [FromForm] string courses)
        {


            var course = courses?.Trim().Trim().Split(";");

            var req3 = new GetCourses();
            var res3 = _requestService.SendGet(req3);

            var coursesList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Course>>(res3.result.ToString());

            var listCourses = new List<Course>();

            if (course != null)
            {
                foreach (var item in course)
                {
                    listCourses.Add(coursesList.FirstOrDefault(m => m.Title == item));
                }
            }

            var user = new Tutor()
            {
                FirstName = firstName,
                LastName = lastName,
                MiddleName = middleName,
                Phone = phone,
                Password = password,
                BirthDate = DateTime.Parse(birthDate),
                Email = email,
                Courses = listCourses,
                Role = "Tutor",
                UserDates = new UserDate() { dateTimes = new List<DateTime>() },

            };
            CustomRequestPost req = new CustomRequestPost("api/tutor/addtutor", user);
            _requestService.SendPost(req, HttpContext);
            return RedirectToAction("Index", "Tutors");
        }
    }
}

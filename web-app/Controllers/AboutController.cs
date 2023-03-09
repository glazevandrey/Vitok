using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.DbContext;
using web_server.Models;

namespace web_app.Controllers
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
            ViewData["courses"] = TestData.Courses;
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(response.result.ToString());
            return View(data);
        }

        [HttpPost("fromregistertologin", Name = "fromregistertologin")]
        public IActionResult FromRegisterToLogin()
        {
            var form = Request.Form;
            var date = new UserDate();
            int tutorId = 0;
            int courseId = 0;

            try
            {
                courseId = Convert.ToInt32(form["course"]);
            }
            catch (Exception ex)
            {
                courseId = -1;
            }

            var tutor = new User();

            if (form.Count != 0)
            {
                tutorId = Convert.ToInt32(form["tutor"]);

                CustomRequestGet req = new GetTutorByIdRequest(tutorId.ToString());
                var response = _requestService.SendGet(req, HttpContext);

                if (response.success == false)
                {
                    return BadRequest("Что-то пошло не так =(");
                }

                tutor = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(response.result.ToString());
                var time = form["textTime"];
                var times = time.ToString().Split(',');

                foreach (var item in times)
                {
                    date.dateTimes.Add(DateTime.Parse(item));
                }
            }

            Registration registration = new Registration
            {
                UserId = TestData.UserList.Last().UserId + 1,
                WantThis = date,
                Course = TestData.Courses.FirstOrDefault(m => m.Id == courseId),
                TutorId = tutorId
            };

            //  пользователь уже зарегисттирован?
            CustomRequestGet request = new GetUserByToken(Request.Cookies[".AspNetCore.Application.Id"]);
            var result = _requestService.SendGet(request, HttpContext);

            if (result.success != false)
            {
                var req = new CustomRequestPost("api/home/addschedulefromuser", registration);
                _requestService.SendPost(req, HttpContext);

                return Redirect($"/login");
            }

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

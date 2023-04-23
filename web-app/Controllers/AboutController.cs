using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using web_app.Requests;
using web_app.Requests.Get;
using web_app.Services;
using web_server.Models.DBModels;

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
            var req2 = new GetCourses();
            var response2 = _requestService.SendGet(req2, HttpContext);

            ViewData["courses"] = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Course>>(response2.result.ToString());
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(response.result.ToString(), Program.settings);
            return View(data);
        }

        [HttpPost("fromregistertologin", Name = "fromregistertologin")]
        public IActionResult FromRegisterToLogin()
        {
            var form = Request.Form;
            var date = new List<UserDate>();
            int tutorId = 0;
            int courseId = -1;

            Int32.TryParse(form["course"], out courseId);

            var tutor = new Tutor();

            if (form.Count != 0)
            {
                tutorId = Convert.ToInt32(form["tutor"]);

                CustomRequestGet req = new GetTutorByIdRequest(tutorId.ToString());
                var response = _requestService.SendGet(req, HttpContext);

                if (response.success == false)
                {
                    return BadRequest("Что-то пошло не так =(");
                }

                tutor = Newtonsoft.Json.JsonConvert.DeserializeObject<Tutor>(response.result.ToString(), Program.settings);
                
                var time = form["textTime"];

                if (string.IsNullOrEmpty(time))
                {
                    return Redirect("/about/tutors");
                }
                var times = time.ToString().Split(',');

                foreach (var item in times)
                {
                    var date22 = DateTime.Parse(item);
                    if((date22 - DateTime.Now).TotalHours <= 24)
                    {
                        date.Add(new UserDate() { dateTime = date22.AddDays(7) });
                    }
                    else
                    {
                        date.Add(new UserDate() { dateTime = date22 });

                    }
                }
            }
            var req2 = new GetCourses();
            var response2 = _requestService.SendGet(req2, HttpContext);

            var courses = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Course>>(response2.result.ToString());

            Registration registration = new Registration
            {
                NewUserGuid = Guid.NewGuid(),
                WantThis = date,
                Course = courses.FirstOrDefault(m => m.Id == courseId),
                TutorId = tutorId
            };

            //  пользователь уже зарегисттирован?
            CustomRequestGet request = new GetUserByToken(Request.Cookies[".AspNetCore.Application.Id"]);
            var result = _requestService.SendGet(request, HttpContext);

            if (result.success != false)
            {
                var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(result.result.ToString(), Program.settings);
                registration.ExistUserId = user.UserId;
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

            var id = registration.ExistUserId.ToString() == "0" ? registration.NewUserGuid.ToString() : registration.ExistUserId.ToString();

            return Redirect($"/login?id={id}");
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
            var tutor = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(response.result.ToString(), Program.settings);
            return View(tutor);
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using web_app.Requests;
using web_app.Requests.Get;
using web_app.Services;
using web_server.Models.DBModels;
using web_server.Services.Interfaces;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/courses")]
    public class CoursesController : Controller
    {
        IJsonService _jsonService;
        IRequestService _requestService;
        public CoursesController(IJsonService jsonService, IRequestService requestService)
        {
            _jsonService = jsonService;
            _requestService = requestService;
        }
        public IActionResult Index(string editError = null)
        {

            CustomRequestGet req = new GetCourses();
            var res = _requestService.SendGet(req);

            var courses = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Course>>(res.result.ToString());

            req = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            res = _requestService.SendGet(req, HttpContext);
            if (res.success == false)
            {
                return Redirect("/login");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString(), Program.settings);
            ViewData["role"] = user.Role;

            if (user.Role == "Student")
            {
                ViewData["lessons"] = ((Student)user).LessonsCount;
            }
            ViewData["usertoken"] = user.UserId;
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;

            var req2 = new GetGoalsRequest();
            var res2 = _requestService.SendGet(req2);
            var goals = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Goal>>(res2.result.ToString());
            ViewData["goals"] = goals;
            if (editError != null)
            {
                ViewData["error"] = editError;
            }
            return View(courses);
        }

        [HttpPost("savenew", Name = "savenew")]
        public IActionResult SaveNew([FromForm] string title, [FromForm] string goalId)
        {
            CustomRequestPost req = new CustomRequestPost("api/servercourses/setnewcourse", $"{title};{goalId}");
            _requestService.SendPost(req, HttpContext);
            return RedirectToAction("Index", "Courses");
        }
        [HttpPost("editCourse", Name = "editCourse")]
        public IActionResult EditCourse([FromForm] string courseIdEdit, [FromForm] string courseTitle, [FromForm] string goalEditId)
        {
            CustomRequestPost req = new CustomRequestPost("api/servercourses/editCourseServer", $"{courseIdEdit};{courseTitle};{goalEditId}");
            _requestService.SendPost(req, HttpContext);
            return RedirectToAction("Index", "Courses");
        }
        [HttpPost("removeCourse", Name = "removeCourse")]
        public IActionResult RemoveCourse([FromForm] string courseId)
        {
            CustomRequestPost req = new CustomRequestPost("api/servercourses/removeCourseServer", $"{courseId}");
            var res = _requestService.SendPost(req, HttpContext);
            if (!res.success)
            {
                return RedirectToAction("Index", "Courses", new
                {
                    editError = "Ошибка при удалении. У одного или нескольких участников платформы указан выбранный курс." +
                    "Чтобы удалить курс, необходимо чтобы ни один из пользователей не ссылался на него."
                });
            }
            return RedirectToAction("Index", "Courses");
        }
    }
}

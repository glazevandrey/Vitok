using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.DbContext;
using web_server.Models;
using web_server.Services;

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
        public IActionResult Index()
        {


            CustomRequestGet req = new GetCourses(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res = _requestService.SendGet(req, HttpContext);
            var courses = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Course>>(res.result.ToString());

            req = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            res = _requestService.SendGet(req, HttpContext);
            if(res.success == false)
            {
                return Redirect("/login");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString());
            ViewData["role"] = user.Role;

            ViewData["goals"] = TestData.Goals;
            return View(courses);
        }

        [HttpPost("savenew", Name ="savenew")]
        public IActionResult SaveNew([FromForm] string title, [FromForm] string goalId)
        {
            CustomRequestPost req = new CustomRequestPost("api/servercourses/setnewcourse", $"{title};{goalId}");
            _requestService.SendPost(req, HttpContext);
            return RedirectToAction("Index", "Courses");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using web_server.DbContext;
using web_server.Models;
using web_server.Services;

namespace web_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServerCoursesController : Controller
    {
        IAuthService _authService;
        IJsonService _jsonService;
        public ServerCoursesController(IAuthService authService, IJsonService jsonService)
        {
            _authService = authService;
            _jsonService = jsonService;
        }


        [HttpGet("getcourses", Name ="getcourses")]
        public string Index()
        {
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(
                            TestData.Courses.ToList()));
        }

        [HttpPost("setnewcourse", Name = "setnewcourse")]
        public string SetNewCourse()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var args = form.First().Key.Split(";");
            var course = new Course();

            course.Title = args[0];
            course.Id = TestData.Courses.Last().Id + 1;
            course.Goal = TestData.Goals.FirstOrDefault(m=>m.Id == Convert.ToInt32(args[1]));

            TestData.Courses.Add(course);

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(
                            TestData.Courses.ToList()));
        }
    }
}

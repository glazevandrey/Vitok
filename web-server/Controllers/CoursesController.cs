﻿using Microsoft.AspNetCore.Mvc;
using System.Linq;
using web_server.DbContext;
using web_server.Services.Interfaces;

namespace web_server.Controllers
{
    [ApiController]
    [Route("api/ServerCourses")]
    public class CoursesController : Controller
    {
        IJsonService _jsonService;
        ICourseService _courseService;
        public CoursesController(IJsonService jsonService, ICourseService courseService)
        {
            _jsonService = jsonService;
            _courseService = courseService;
        }


        [HttpGet("getcourses", Name = "getcourses")]
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

            var json = _courseService.SetNewCourse(args);
            if (json == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка добавить курс");
            }

            return _jsonService.PrepareSuccessJson(json);
        }
    }
}
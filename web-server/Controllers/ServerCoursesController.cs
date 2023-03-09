﻿using Microsoft.AspNetCore.Mvc;
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
        IJsonService _jsonService;
        ICourseService _courseService;
        public ServerCoursesController(IJsonService jsonService, ICourseService courseService)
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

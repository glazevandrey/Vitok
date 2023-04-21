﻿using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using web_server.Services.Interfaces;

namespace web_server.Controllers
{
    [ApiController]
    [Route("api/ServerCourses")]
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
        public async Task<string> Index()
        {
            var res = await _courseService.GetCourses();
            string json = "";
            try
            {
                json = Newtonsoft.Json.JsonConvert.SerializeObject(res);

            }
            catch (System.Exception ex)
            {

                throw ex;
            }
            return _jsonService.PrepareSuccessJson(json);
        }

        [HttpPost("setnewcourse", Name = "setnewcourse")]
        public async Task<string> SetNewCourse()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var args = form.First().Key.Split(";");

            var json = await _courseService.SetNewCourse(args);
            if (json == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка добавить курс");
            }

            return _jsonService.PrepareSuccessJson(json);
        }


        [HttpPost("removeCourseServer", Name = "removeCourseServer")]
        public async Task<string> RemoveCourseServer()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var args = form.First().Key;

            var json = await _courseService.RemoveCourse(args);
            if (json == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка добавить курс");
            }

            return _jsonService.PrepareSuccessJson(json);
        }
        [HttpPost("editCourseServer", Name = "editCourseServer")]
        public async Task<string> EditCourseServer()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var args = form.First().Key.Split(";");

            var json = await _courseService.EditCourse(args);
            if (json == null)
            {
                return _jsonService.PrepareErrorJson("Неудачная попытка добавить курс");
            }

            return _jsonService.PrepareSuccessJson(json);
        }
    }
}

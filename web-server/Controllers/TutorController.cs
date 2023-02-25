using Microsoft.AspNetCore.Mvc;
using web_server.DbContext;
using web_server.Services;
using System;
using System.Linq;
using web_server.Models;
using System.Collections.Generic;
using Microsoft.OpenApi.Validations;

namespace vitok.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TutorController : Controller
    {
        IJsonService _jsonService;
        public TutorController(IJsonService jsonService)
        {
            _jsonService = jsonService;
        }
        [HttpGet("getall", Name = "GetAll")]
        public string GetAllTutors()
        {
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(TestData.Tutors));
        }

        [HttpGet("gettutor", Name = "gettutor")]
        public string GetTutor([FromQuery] string args)
        {
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(
                TestData.Tutors.FirstOrDefault(m => m.Id.ToString() == args)));
        }
        [Authorize]
        [HttpPost("addtutorfreedate", Name = "addtutorfreedate")]
        public string AddTutorFreeDate()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;
            var split = args.Split(';');
            var tutor_id = split[0];
            var dateTime = DateTime.Parse(split[1]);
            var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id));
            if (tutor != null)
            {
                TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id)).UserDates.dateTimes.Add(dateTime);
                TestData.Schedules.Add(new Schedule()
                {
                    Id  = TestData.Schedules.Last().Id+1,
                    Date = new UserDate() { dateTimes = new List<DateTime>() { dateTime} },
                    Looped = Convert.ToBoolean(split[2]),
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    TutorId = tutor.Id,
                    UserId = -1,
                });

                return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
            }
            return _jsonService.PrepareErrorJson("Tutor not found");
        }

        [Authorize]
        [HttpPost("addtutorschedule", Name = "addtutorschedule")]
        public string AddTutorSchedule()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;
            var split = args.Split(';');
            var tutor_id = split[0];
            var user_id = split[3];
            var course_id = split[4];

            var dateTime = DateTime.Parse(split[1]);
            var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id));
            var user = TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(user_id));
            var course = TestData.Courses.FirstOrDefault(m=>m.Id == Convert.ToInt32(course_id));
            if (tutor != null)
            {
                tutor.UserDates.dateTimes.Add(dateTime);
                TestData.Schedules.Add(new Schedule()
                {
                    Id = TestData.Schedules.Last().Id + 1,
                    Date = new UserDate() { dateTimes = new List<DateTime>() { dateTime } },
                    UserName = user.FirstName + " " + user.LastName,
                    Looped = Convert.ToBoolean(split[2]),
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    TutorId = tutor.Id,
                    UserId = user.UserId,
                    Course = course
                }) ;

                return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
            }
            return _jsonService.PrepareErrorJson("Tutor not found");
        }
        [Authorize]
        [HttpPost("removetutortimeandschedule", Name = "removetutortimeandschedule")]
        public string RemoveTutorTimeAndSchedule()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;

            var split = args.Split(';');
            var dateTime = DateTime.Parse(split[2]);
            var tutor_id = split[0];
            var user_id = split[1];

            var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id));
            if (tutor != null)
            {
                TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id)).UserDates.dateTimes.Remove(dateTime);
                if (TestData.Schedules.FirstOrDefault(m => m.TutorId == Convert.ToInt32(tutor_id) && m.Date.dateTimes[0] == dateTime && m.UserId == Convert.ToInt32(user_id)) != null)
                {
                    TestData.Schedules.Remove(TestData.Schedules.FirstOrDefault(m => m.TutorId == Convert.ToInt32(tutor_id) && m.Date.dateTimes[0] == dateTime && m.UserId == Convert.ToInt32(user_id)));
                }
                return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
            }
            return _jsonService.PrepareErrorJson("Tutor not found");

        }
        [HttpPost("removetutortime", Name = "removetotortime")]
        public string RemoveTutorTime()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;

            var split = args.Split(';');
            var tutor_id = split[0];
            var dateTime = DateTime.Parse(split[1]);
            var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id));
            if (tutor != null)
            {
                TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id)).UserDates.dateTimes.Remove(dateTime);
                //if(TestData.Schedules.FirstOrDefault(m=>m.TutorId == Convert.ToInt32(tutor_id) && m.Date.dateTimes[0] == dateTime) != null)
                //{
                //    TestData.Schedules.Remove(TestData.Schedules.FirstOrDefault(m => m.TutorId == Convert.ToInt32(tutor_id) && m.Date.dateTimes[0] == dateTime));
                //}
                return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
            }
            return _jsonService.PrepareErrorJson("Tutor not found");
        }
    }
}

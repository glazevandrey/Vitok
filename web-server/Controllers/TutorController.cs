using Microsoft.AspNetCore.Mvc;
using web_server.DbContext;
using web_server.Services;
using System;
using System.Linq;

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
                tutor.UserDates.dateTimes.Remove(dateTime);
                return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
            }
            return _jsonService.PrepareErrorJson("Tutor not found");
        }
    }
}

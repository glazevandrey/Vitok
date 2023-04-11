using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using web_server.Database;
using web_server.Database.Repositories;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;
using web_server.Services.Interfaces;

namespace web_server.Controllers
{

    [ApiController]
    [Route("/api/[controller]")]
    public class HomeController : Controller
    {
        private readonly IHubContext<NotifHub> _hubContext;
        IAuthService _authService;
        IJsonService _jsonService;
        ILessonsService _lessonsService;
        IScheduleService _scheduleService;
        IStatisticsService _statisticsService;
        UserRepository _userRepository;
        ContactsRepository _contactsRepository;
        CourseRepository _courseRepository;
        public HomeController(UserRepository userRepository, CourseRepository courseRepository, ContactsRepository contactsRepository, IStatisticsService statisticsService ,IAuthService authService, IHubContext<NotifHub> hub, IJsonService jsonService, ILessonsService lessonsService, IScheduleService scheduleService)
        {
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _contactsRepository = contactsRepository;
            _hubContext = hub;
            _authService = authService;
            _jsonService = jsonService;
            _lessonsService = lessonsService;
            _scheduleService = scheduleService;
            _statisticsService = statisticsService;
        }

        [HttpPost("loginuser", Name = "loginuser")]
        public async Task<string> LoginUser()
        {

            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }
            var args = form.Keys.First().Split(";");
            var json =  await _authService.LogIn(args[0], args[1], HttpContext);
          
            return json;
        }

        [HttpPost("registeruser", Name = "registeruser")]
        public async Task<string> RegisterUser()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(form.First().Key);
            var json = await _authService.Register(user, HttpContext, _hubContext);
            return json;
        }

        [HttpGet("getAllSchedules", Name = "getAllSchedules")]
        public async Task<string> GetAllSchedules()
        {
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(await _scheduleService.GetAllSchedules()));
        }

        [HttpGet("getAllReSchedules", Name = "getAllReSchedules")]
        public async Task<string> GetAllReSchedules()
        {
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(await _scheduleService.GetAllReschedules()));
        }

        [Authorize]
        [HttpGet("getuser", Name = "getuser")]
        public async Task<string> GetUser([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var json = await _authService.GetUserByToken(args);
            var check = await _authService.CheckIsActive(HttpContext);
            if (check == false)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            return json;
        }

        [HttpGet("getAllStudentsAndTutors", Name = "getAllStudentsAndTutors")]
        public async Task<string> GetAll([FromQuery] string args)
        {
            var all = await _userRepository.GetAll();
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(all));
        }

        [Authorize]
        [HttpGet("getuserbyid", Name = "getuserbyid")]
        public async Task<string> GetUserById([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var json = await _authService.GetUserById(args);
            var check = await _authService.CheckIsActive(HttpContext);
            if (check == false)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");

            }

            return json;
        }



        [Authorize]
        [HttpGet("getstatistics", Name = "getstatistics")]
        public string Getstatistics([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var data = _statisticsService.FormingStatData(args);
            if (data == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");

            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(data));
        }


        [Authorize]
        [HttpGet("getschedulebyid", Name = "getschedulebyid")]
        public async Task<string> GetScheduleById([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var userSchedules = _scheduleService.GetSchedules(args[0].ToString()); //TestData.Schedules.Where(m => m.UserId == Convert.ToInt32(args[0].ToString())).ToList();

            //var userSchedules = TestData.Schedules.Where(m => m.UserId == Convert.ToInt32(args[0].ToString())).ToList();

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(userSchedules));
        }

        [HttpPost("adduserregistration", Name = "adduserregistration")]
        public async Task<string> AddUserRegistration()
        {
            var form = Request.Form;
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Registration>(form.First().Key);

            if (form.Keys.Count != 0)
            {
                await _authService.AddRegistration(model);
                var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                return json;
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }

        [Authorize]
        [HttpPost("addschedulefromuser", Name = "addschedulefromuser")]
        public string AddScheduleFromUser()
        {
            var form = Request.Form;

            var schedule = _scheduleService.AddScheduleFromUser(form.First().Key, _hubContext);
            if (schedule != null)
            {
                var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(schedule));
                return json;
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }

        [HttpGet("getregistration", Name = "getregistration")]
        public async Task<string> GetRegistration([FromQuery] string args)
        {
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(await _userRepository.GetRegistrationByUserId(Convert.ToInt32(args))));
        }

        [HttpGet("getTariffs", Name ="getTariffs")]
        public async Task<string> GetTariffs()
        {
            var tariffs = await _courseRepository.GetAllTariffs();
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tariffs));
        }

        [HttpGet("getGoals", Name = "getGoals")]
        public async Task<string> GetGoals()
        {
            var goals = await _courseRepository.GetAllGoals();
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(goals));
        }
        [HttpPost("getcontacts", Name = "GetContacts")]
        public async Task<string> GetContacts()
        {
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(await _contactsRepository.GetContacts()));
        }

    }
}

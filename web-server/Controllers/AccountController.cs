using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Services.Interfaces;

namespace web_server.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IJsonService _jsonService;
        private readonly ILessonsService _lessonsService;
        private readonly IAccountService _accountService;
        private readonly IScheduleService _scheduleService;
        private readonly IMapper _mapper;

        NotificationRepository _notificationRepository;
        public AccountController(IMapper mapper, IJsonService jsonService, ILessonsService lessonsService, IAccountService accountService, IScheduleService scheduleService, NotificationRepository notificationRepository)
        {
            _mapper = mapper;
            _notificationRepository = notificationRepository;
            _jsonService = jsonService;
            _lessonsService = lessonsService;
            _accountService = accountService;
            _scheduleService = scheduleService;
        }

        [Models.Authorize]
        [HttpPost("saveuserinfo", Name = "saveuserinfo")]
        public async Task<string> SaveUserInfo()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var user = await _accountService.SaveAccountInfo(form.First().Key);
            if (user != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                return _jsonService.PrepareSuccessJson(json);
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }

        [HttpGet("getalltutorstudents", Name = "getalltutorstudents")]
        public async Task<string> GetAllTutorStudents([FromQuery] string args)
        {
            
            var users = await _accountService.GetAllUserContacts(args, "Tutor");
            //var user = await _accountService.SaveAccountInfo(form.First().Key);
            if (users != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(users);
                return _jsonService.PrepareSuccessJson(json);
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }
        
        [HttpGet("getallstudenttutors", Name = "getallstudenttutors")]
        public async Task<string> GetAllStudentTutors([FromQuery] string args)
        {
            var users = await _accountService.GetAllUserContacts(args, "Student");

            //var user = await _accountService.SaveAccountInfo(form.First().Key);
            if (users != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(users);
                return _jsonService.PrepareSuccessJson(json);
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }

        [Models.Authorize]
        [HttpPost("removeFirstLogin", Name = "removeFirstLogin")]
        public async Task<string> RemoveFirstLogin()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var args = form.First().Key;

            var user = await _accountService.RemoveFirstLogin(args);
            if (user == false)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                return _jsonService.PrepareSuccessJson(json);
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }

        [Models.Authorize]
        [HttpPost("addlessons", Name = "addlessons")]
        public async Task<string> AddLessons()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var args = form.First().Key.Split(";");

            var user = await _lessonsService.AddLessonsToUser(args);
            if (user != null)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                return _jsonService.PrepareSuccessJson(json);
            }

            return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
        }

        [HttpPost("savephoto", Name = "savephoto")]
        public IActionResult SavePhoto([FromQuery] string id)
        {
            var file = Request.Form.Files[0];

            _accountService.SavePhoto(file, id);

            return Redirect($"{Program.web_app_ip}/account");
        }



        [Authorize]
        [HttpPost("TutorWithdraw")]
        public async Task<string> TutorWithdraw()
        {
            var args = Request.Form.First().Key?.Split(";");
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка!");
            }

            var res = await _accountService.Withdraw(args[0], args[1]);
            if (res == false)
            {
                return _jsonService.PrepareErrorJson("Недостаточно средств");
            }
            return _jsonService.PrepareSuccessJson("true");
        }

        [HttpPost("MarkAsRead", Name = "MarkAsRead")]
        public async Task MarkAsRead([FromForm] int id)
        {
            var sc = await _notificationRepository.GetNotification(id);
            // var sc = TestData.Notifications.FirstOrDefault(m => m.Id == Convert.ToInt32(id));
            if (sc == null)
            {
                return;
            }

            sc.Readed = true;

            await _notificationRepository.UpdateNotification();
        }

        [Models.Authorize]
        [HttpGet("getschedule", Name = "getschedule")]
        public async Task<string> GetSchedule([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка!");
            }

            var list = await _scheduleService.GetSchedules(args);
            string json = "";
            try
            {
                json = Newtonsoft.Json.JsonConvert.SerializeObject(list);
            }
            catch (Exception ex)
            {
                try
                {
                    json = Newtonsoft.Json.JsonConvert.SerializeObject(_mapper.Map<List<Schedule>>(list));


                }
                catch (Exception x)
                {

                    throw x;
                }
            }

            return _jsonService.PrepareSuccessJson(json);

        }
    }
}

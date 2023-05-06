using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using web_server.Database;
using web_server.Database.Repositories;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;
using web_server.Services;
using web_server.Services.Interfaces;

namespace web_server.Controllers
{

    [ApiController]
    [Route("/api/[controller]")]
    public class HomeController : Controller
    {
        private readonly IHubContext<NotifHub> _hubContext;
        private readonly IAuthService _authService;
        private readonly IJsonService _jsonService;
        private readonly IScheduleService _scheduleService;
        private readonly IStatisticsService _statisticsService;
        private readonly UserRepository _userRepository;
        private readonly ContactsRepository _contactsRepository;
        private readonly CourseRepository _courseRepository;
        private readonly DataContext data;
        private readonly IMapper map;
        public HomeController(IMapper maз, DataContext data, UserRepository userRepository, CourseRepository courseRepository, ContactsRepository contactsRepository, IStatisticsService statisticsService, IAuthService authService, IHubContext<NotifHub> hub, IJsonService jsonService, IScheduleService scheduleService)
        {
            map = maз;
            this.data = data;
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _contactsRepository = contactsRepository;
            _hubContext = hub;
            _authService = authService;
            _jsonService = jsonService;
            _scheduleService = scheduleService;
            _statisticsService = statisticsService;
        }

        [HttpPost("loginuser", Name = "loginuser")]
        public async Task<string> LoginUser()
        {
            await InitializeDataBase();
            var form = Request.Form;

            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var args = form.Keys.First().Split(";");
            var json = await _authService.LogIn(args[0], args[1], HttpContext);

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
            var id = Request.Query["id"];
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<Student>(form.First().Key);
            var json = await _authService.Register(user, id, HttpContext, _hubContext);
            return json;
        }

        [HttpGet("getAllSchedules", Name = "getAllSchedules")]
        public async Task<string> GetAllSchedules()
        {
            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(map.Map<List<Schedule>>(await _scheduleService.GetAllSchedules())));
        }





        [Authorize]
        [HttpGet("getliteuser", Name = "getliteuser")]
        public async Task<string> GetLiteUser([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var json = await _authService.GetLiteUserByToken(args);

            return json;
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

            return json;
        }

        [HttpGet("getAllStudentsAndTutors", Name = "getAllStudentsAndTutors")]
        public async Task<string> GetAll([FromQuery] string args)
        {
            var all = await _userRepository.GetAll();
            string json = "";
            try
            {
                json = (Newtonsoft.Json.JsonConvert.SerializeObject(all));
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return _jsonService.PrepareSuccessJson(json);
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
            return json;
        }

        [HttpGet("getSiteContacts", Name = "getSiteContacts")]
        public async Task<string> GetSiteContacts()
        {

            var json = _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(await data.SiteContacts.ToListAsync()));
            return json;
        }

        
                [Authorize]
        [HttpGet("getstatistics", Name = "getstatistics")]
        public async Task<string> Getstatistics([FromQuery] string args)
        {
            if (args == null)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }

            var data = await _statisticsService.FormingStatData(args);
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

            var userSchedules = await _scheduleService.GetSchedules(args[0].ToString()); //TestData.Schedules.Where(m => m.UserId == Convert.ToInt32(args[0].ToString())).ToList();

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
        public async Task<string> AddScheduleFromUser()
        {
            var form = Request.Form;

            var schedule = await _scheduleService.AddScheduleFromUser(form.First().Key, _hubContext);
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

        [HttpGet("getTariffs", Name = "getTariffs")]
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

        private async Task InitializeDataBase()
        {
            if (data.Tutors.Count() == 0 && data.Students.Count() == 0 && data.Managers.Count() == 0)
            {
                data.SiteContacts.AddRange(TestData.Sites);


                data.Goals.AddRange(TestData.Goals);
                try
                {
                    data.SaveChanges();

                }
                catch (Exception ex)
                {

                    throw ex;
                }

                data.Courses.AddRange(TestData.Courses);
                try
                {
                    data.SaveChanges();
                    foreach (var item in TestData.Courses)
                    {
                        data.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }

                var gg = new StudentDTO()
                {

                    Role = "Student",

                };
                data.Students.Add(gg);

                data.SaveChanges();
                data.Entry(gg).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                var students = TestData.UserList.Where(m => m.Role == "Student");
                foreach (var item in students)
                {
                    data.Students.Add(map.Map<StudentDTO>(item));
                }

                data.SaveChanges();

                foreach (var item in students)
                {
                    data.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                }
                var tutors = TestData.UserList.Where(m => m.Role == "Tutor");
                foreach (var item in tutors)
                {
                    var d = map.Map<TutorDTO>(item);
                    //foreach (var dd in d.Courses)
                    //{
                    //    dd.Id = 0;
                    //}
                    if (d.About.ToLower().Contains("огэ"))
                    {
                        d.Courses.Add(new TutorCourse() { CourseId = TestData.Courses.FirstOrDefault(m => m.Title == "ОГЭ").Id });
                    }
                    else
                    {
                        d.Courses.Add(new TutorCourse() { CourseId = TestData.Courses.FirstOrDefault(m => m.Title == "Общий английский").Id });

                    }
                    if (d.About.ToLower().Contains("егэ"))
                    {
                        d.Courses.Add(new TutorCourse() { CourseId = TestData.Courses.FirstOrDefault(m => m.Title == "ЕГЭ").Id });
                    }
                    
                    
                    data.Tutors.Add(d);
                }
                try
                {
                    data.SaveChanges();
                }
                catch (Exception ex)
                {

                    throw ex;
                }
                var manager = TestData.UserList.FirstOrDefault(m => m.Role == "Manager");
                data.Managers.Add(map.Map<ManagerDTO>(manager));

                try
                {

                    data.SaveChanges();

                    foreach (var item in tutors)
                    {
                        data.Entry(map.Map<TutorDTO>(item)).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    }

                    data.Entry(map.Map<ManagerDTO>(manager)).State = Microsoft.EntityFrameworkCore.EntityState.Detached;


                }
                catch (Exception ex)
                {

                    throw ex;
                }


                foreach (var item in TestData.Schedules)
                {
                    item.CourseId = TestData.Courses.First(m => m.Title == "ОГЭ").Id;
                }
                data.Schedules.AddRange(map.Map<List<ScheduleDTO>>(TestData.Schedules));
                data.Tariffs.AddRange(TestData.Tariffs);
                try
                {
                    data.SaveChanges();
                    foreach (var item in map.Map<List<ScheduleDTO>>(TestData.Schedules))
                    {

                        data.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    }

                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }


        }
        private List<ScheduleDTO> GetTen()
        {
            return data.Schedules.Include(m => m.RescheduledLessons).Include(m => m.ReadyDates).Include(m => m.PaidLessons).Include(m => m.SkippedDates).Take(10).ToList();
        }
        private void TestSort()
        {
            var res = new List<ScheduleDTO>();
            int count = 1000; // * 10

            for (int i = 0; i < count; i++)
            {
                res.AddRange(GetTen());
            }
            Stopwatch s = Stopwatch.StartNew();
            ScheduleService.SortSchedulesForUnpaid(res);
            s.Stop();
            // 100 -  00565
            // 1000 - 00822
            // 10000
            var time = s.Elapsed;
        }

    }
}

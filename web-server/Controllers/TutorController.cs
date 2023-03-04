using Microsoft.AspNetCore.Mvc;
using web_server.DbContext;
using web_server.Services;
using System;
using System.Linq;
using web_server.Models;
using System.Collections.Generic;
using Microsoft.OpenApi.Validations;
using System.Data;
using System.Globalization;

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
        [HttpPost("addtutor", Name = "addtutor")]
        public string AddTutor()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var raw = form.First().Key;
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(raw.ToString());
            model.Id = TestData.UserList.Last().Id + 1;
            model.UserId = TestData.UserList.Last().UserId + 1;

            TestData.UserList.Add(model);

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(model));
        }

        [Authorize]
        [HttpPost("updatetutordata", Name = "updatetutordata")]
        public string updatetutordata()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(form.First().Key);
            var old = TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId);

            old.FirstName = user.FirstName;
            old.LastName = user.LastName;
            old.MiddleName = user.MiddleName;
            old.BirthDate = user.BirthDate;
            old.About = user.About;
            old.Email = user.Email;
            old.Wish = user.Wish;
            old.Courses = user.Courses;
            old.Password = user.Password;
            old.Phone = user.Phone;

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(user));
        }

        [Authorize]
        [HttpPost("removeTutorServer", Name = "removeTutorServer")]
        public string RemoveTutor()
        {
            var form = Request.Form;
            if (form == null || form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Возникла непредвиденная ошибка");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(form.First().Key);
            if(user!= null)
            {
                TestData.Tutors.RemoveAll(m=>m.UserId == user.UserId);
                if (TestData.UserList.FirstOrDefault(m=>m.UserId == user.UserId) != null)
                {
                    TestData.UserList.RemoveAll(m=>m.UserId == user.UserId);
                    return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject("OK"));

                }
            }

            return _jsonService.PrepareErrorJson(Newtonsoft.Json.JsonConvert.SerializeObject("BAD RESULT"));
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
                    StartDate = dateTime,
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
                    Course = course,
                    Status = Status.Ожидает,
                    StartDate = dateTime
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
            var curr = DateTime.Parse(split[3]);

            var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id));
            if (tutor != null)
            {
                TestData.Tutors.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutor_id)).UserDates.dateTimes.Remove(dateTime);
               
                if (TestData.Schedules.FirstOrDefault(m => m.TutorId == Convert.ToInt32(tutor_id) && m.Date.dateTimes[0] == dateTime && m.UserId == Convert.ToInt32(user_id)) != null)
                {
                    TestData.Schedules.FirstOrDefault(m => m.TutorId == Convert.ToInt32(tutor_id) && m.Date.dateTimes[0] == dateTime && m.UserId == Convert.ToInt32(user_id)).RemoveDate = curr;
                }
                return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
            }
            return _jsonService.PrepareErrorJson("Tutor not found");

        }

        [Authorize]
        [HttpPost("changeStatusServer", Name = "changeStatusServer")]
        public string ChangeStatus()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;

            var split = args.Split(';');
            var status = split[0];
            var tutor_id = Convert.ToInt32(split[1]);
            var user_id = Convert.ToInt32(split[2]);
            var date = DateTime.Parse(split[3]);
            var dateCurr = DateTime.Parse(split[4]);

            var model = TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == date);
            
            if((Status)Enum.Parse(typeof(Status), status) == Status.Проведен)
            {
                TestData.UserList.FirstOrDefault(m=>m.UserId == user_id).LessonsCount--;
                TestData.UserList.FirstOrDefault(m => m.UserId == user_id).BalanceHistory.CustomMessages.Add(DateTime.Now, "-1 занятие");



                TestData.Tutors.FirstOrDefault(m=>m.UserId == tutor_id).Balance += 1000;
                TestData.Tutors.FirstOrDefault(m => m.UserId == tutor_id).BalanceHistory.CashFlow.Add(new CashFlow() { Date = DateTime.Now, Amount = 1000});

                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == date).Tasks[Constatnts.NOTIF_START_LESSON] = false;
                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == date).Tasks[Constatnts.NOTIF_TOMORROW_LESSON] = false;

            }

            if (model.Looped)
            {
                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == date).ReadyDates.Add(dateCurr);
            }
            else
            {
                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == date).Status = (Status)Enum.Parse(typeof(Status), status);
            }

            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(""));
        }

        [Authorize]
        [HttpPost("rescheduletutor", Name = "rescheduletutor")]
        public string RescheduleTutor()
        {
            var form = Request.Form;
            if (form.Keys.Count == 0)
            {
                return _jsonService.PrepareErrorJson("Tutor not found");
            }
            var args = form.First().Key;

            var split = args.Split(';');

            //    status = 0
            //    tut = 1
            //    dateOld = 2
            //    loop = 3
            //    user = 4
            //    newdate = 5
            //    reason = 6
            //    initiator = 7
            //    newTime = 8
            //    courseId = 9

            var oldDateTime = DateTime.Parse(split[2]);
            var newDateTime = DateTime.Parse(split[5] + " " + split[8]);
            var status = split[0];

            var tutor_id = Convert.ToInt32(split[1]);
            var user_id = Convert.ToInt32(split[4]);
            var loop = split[3];
            var initiator = split[7];
            var reason = split[6];
            var courseId = Convert.ToInt32(split[9]);
            var cureDate = DateTime.Parse(split[10]);
            var tutor = TestData.Tutors.FirstOrDefault(m => m.UserId == tutor_id);
            var user = TestData.UserList.FirstOrDefault(m=>m.UserId == user_id);
        
            if (Convert.ToBoolean(loop))
            {
                var new_model = new Schedule
                {
                    TutorId = tutor_id,
                    UserId = user_id,
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    UserName = user.FirstName + " " + user.LastName,
                    Course = TestData.Courses.FirstOrDefault(m=>m.Id == courseId),
                    Date  = new UserDate() { dateTimes = new List<DateTime>() { newDateTime} },
                    Id = TestData.Schedules.Last().Id +1,
                    StartDate = newDateTime,
                    Looped  = true,
                };

                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).Status = Status.Перенесен;
                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).RescheduledId = new_model.Id;
                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).RescheduledDate = cureDate;

                TestData.Schedules.Add(new_model);
            }
            else
            {
                var model = new RescheduledLessons
                {
                    Initiator = initiator,
                    NewTime = newDateTime,
                    OldTime = oldDateTime,
                    Reason = reason,
                    TutorId = tutor_id,
                    UserId = user_id
                };

                var new_model = new Schedule
                {
                    TutorId = tutor_id,
                    UserId = user_id,
                    TutorFullName = tutor.FirstName + " " + tutor.LastName,
                    UserName = user.FirstName + " " + user.LastName,
                    Course = TestData.Courses.FirstOrDefault(m => m.Id == courseId),
                    Date = new UserDate() { dateTimes = new List<DateTime>() { newDateTime } },
                    Id = TestData.Schedules.Last().Id + 1,
                    StartDate = newDateTime,
                    Looped = false,
                };

                var re_less = new RescheduledLessons() { Initiator= initiator, NewTime = newDateTime, OldTime = cureDate, Reason = reason, TutorId = tutor_id, UserId = user_id};

                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).Status = Status.Перенесен;

                TestData.Schedules.FirstOrDefault(m => m.TutorId == tutor_id && m.UserId == user_id && m.Date.dateTimes[0] == oldDateTime).RescheduledLessons.Add(re_less);
                
                TestData.Schedules.Add(new_model);
            }



            return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
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

                return _jsonService.PrepareSuccessJson(Newtonsoft.Json.JsonConvert.SerializeObject(tutor.UserDates));
            }
            return _jsonService.PrepareErrorJson("Tutor not found");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using web_app.Models;
using web_app.Models.Requests;
using web_app.Models.Requests.Get;
using web_app.Services;
using web_server.DbContext;
using web_server.Models;
using web_server.Services.Interfaces;

namespace web_app.Controllers
{
    [ApiController]
    [Route("/students")]
    public class StudentsController : Controller
    {
        IJsonService _jsonService;
        IRequestService _requestService;
        public StudentsController(IJsonService jsonService, IRequestService requestService)
        {
            _jsonService = jsonService;
            _requestService = requestService;
        }
        public IActionResult Index()
        {
            CustomRequestGet req4 = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res4 = _requestService.SendGet(req4, HttpContext);

            if (!res4.success)
            {
                if (!string.IsNullOrEmpty(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]))
                {
                    return Redirect("/login");

                }
                else
                {
                    return Redirect("/logout");

                }
            }
            var currUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res4.result.ToString());
            ViewData["usertoken"] = currUser.UserId;
            ViewData["photoUrl"] = currUser.PhotoUrl;
            ViewData["displayName"] = currUser.FirstName + " " + currUser.LastName;

            var req = new GetAllUsersRequest();
            var res = _requestService.SendGet(req, HttpContext);
            if (!res.success)
            {
                return Redirect("/login");
            }
            ViewData["role"] = "Manager";
            var users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(res.result.ToString());
            users = users.Where(m => m.Role == "Student").ToList();
            var schedules = new List<Schedule>();

            foreach (var user in users)
            {
                var req2 = new GetSchedulesByUserId(user.UserId.ToString());
                res = _requestService.SendGet(req2, HttpContext);

                if (res.success)
                {
                    schedules.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(res.result.ToString()));
                }

            }

            ViewData["schedules"] = schedules;
            return View(users);
        }


        [HttpGet("statistics", Name = "statistics")]
        public IActionResult Statistics([FromQuery] string userid)
        {
            CustomRequestGet req = new GetUserById(userid + ";" + "Manager");
            var res = _requestService.SendGet(req, HttpContext);
            if (!res.success)
            {
                return Redirect("/login");
            }
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res.result.ToString());

            ViewData["role"] = user.Role;
            ViewData["tariffs"] = TestData.Tariffs;
            ViewData["usertoken"] = user.UserId;
            ViewData["lessons"] = user.LessonsCount;
            ViewData["photoUrl"] = user.PhotoUrl;
            ViewData["displayName"] = user.FirstName + " " + user.LastName;
            if (user.FirstLogin == true && user.Role == "Student")
            {
                ViewData["firstLogin"] = true;
            }


            var list = new List<StudentPayment>();
            var req2 = new GetSchedulesByUserId(user.UserId.ToString());
            var res2 = _requestService.SendGet(req2, HttpContext);
            var schedules = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(res2.result.ToString());

            var unpaidLessons = new List<DateTime>(); // список долговых занятий
            var paidLessons = new List<DateTime>(); // список оплаченных занятий

            var startDate = DateTime.Parse("20.03.2023 12:00");
            var ff = user.BalanceHistory;
            ff.Reverse();

            var gg = ff.GroupBy(x => x.Date.ToShortDateString()).ToList();
            for (int i = 0; i < gg.Count; i++)
            {
                var list2 = gg[0].ToList();
                for (int j = 0; j < list2.Count; j++)
                {
                    if (list2[j].CashFlow == null)
                    {
                        continue;
                    }
                    var date = ff[i].Date;
                    if (date < startDate)
                    {
                        continue;
                    }

                    DateTime date2 = DateTime.MaxValue;
                    if (gg.Count > 1)
                    {
                        if ((i + 1) == ff.Count)
                        {
                            date2 = DateTime.MaxValue;
                        }
                        else
                        {

                            date2 = ff[i + 1].Date;
                            if (date2.ToShortDateString() == date.ToShortDateString())
                            {
                                date2 = DateTime.MaxValue;
                            }
                        }
                    }


                    foreach (var item in schedules)
                    {
                        if (item.Looped)
                        {
                            foreach (var ready in item.ReadyDates)
                            {


                                if (ready >= date && ready < date2)
                                {
                                    if (list.Count == 0)
                                    {

                                        list.Add(new StudentPayment()
                                        {
                                            LessonAmount = 1000,
                                            PaymentDate = date,
                                            PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        });
                                    }
                                    else
                                    {
                                        if (list.FirstOrDefault(m => m.LessonDate == ready) != null)
                                        {
                                            continue;
                                        }
                                        list.Add(new StudentPayment()
                                        {
                                            LessonAmount = 1000,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        });

                                    }
                                }
                                else if (ready >= date2 && (date2 != DateTime.MinValue))
                                {
                                    if (list.FirstOrDefault(m => m.LessonDate == ready) != null)
                                    {
                                        continue;
                                    }
                                    list.Add(new StudentPayment()
                                    {
                                        LessonAmount = 1000,
                                        PaymentDate = date,
                                        PaymentAmount = list2[j].CashFlow.Amount,
                                        StudentName = item.UserName,
                                        LessonDate = ready
                                    });
                                }
                            }

                        }
                        else
                        {
                            if (item.Status == Status.Проведен)
                            {
                                var ready = item.EndDate;

                                if (ready >= date && ready < date2)
                                {
                                    if (list.Count == 0)
                                    {
                                        list.Add(new StudentPayment()
                                        {
                                            LessonAmount = 1000,
                                            PaymentDate = date,
                                            PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        });
                                    }
                                    else
                                    {
                                        if (list.FirstOrDefault(m => m.LessonDate == ready) != null)
                                        {
                                            continue;
                                        }
                                        list.Add(new StudentPayment()
                                        {
                                            LessonAmount = 1000,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        });

                                    }
                                }
                                else if (ready >= date2 && (date2 != DateTime.MinValue))
                                {
                                    if (list.FirstOrDefault(m => m.LessonDate == ready) != null)
                                    {
                                        continue;
                                    }
                                    list.Add(new StudentPayment()
                                    {
                                        LessonAmount = 1000,
                                        PaymentDate = date,
                                        PaymentAmount = list2[j].CashFlow.Amount,
                                        StudentName = item.UserName,
                                        LessonDate = ready
                                    });
                                }

                            }
                        }
                    }
                }

            }

            return View(list);
        }

        [HttpGet("info", Name = "info")]
        public IActionResult Info([FromQuery] string id)
        {
            CustomRequestGet req4 = new GetUserByToken(HttpContext.Request.Cookies[".AspNetCore.Application.Id"]);
            var res4 = _requestService.SendGet(req4, HttpContext);

            if (!res4.success)
            {
                return Redirect("/login");
            }
            var currUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(res4.result.ToString());
            ViewData["usertoken"] = currUser.UserId;
            ViewData["photoUrl"] = currUser.PhotoUrl;
            ViewData["role"] = "Manager";




            var req = new GetUserById(id + ";Manager");
            var data = _requestService.SendGet(req, HttpContext);
            if (!data.success)
            {
                return Redirect("/login");
            }

            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(data.result.ToString());


            //foreach (var item in ff)
            //{
            //    var date = item.Date;
            //    if(date < startDate)
            //    {
            //        continue;
            //    }

            //    int count = Math.Abs(item.CashFlow.Amount / 1000);



            //    foreach (var lesson in schedules)
            //    {
            //        foreach (var da in lesson.ReadyDates)
            //        {
            //            if(da > date)
            //            {
            //                if(list.Count == 0)
            //                {
            //                    var sch = new StudentPayment()
            //                    {
            //                        LessonAmount = 1000,
            //                        PaymentDate = date,
            //                        PaymentAmount = item.CashFlow.Amount,
            //                        StudentName = lesson.UserName,
            //                        LessonDate = da
            //                    };
            //                }

            //            }
            //        }

            //    }

            //}



            return View(user);
        }
    }
}

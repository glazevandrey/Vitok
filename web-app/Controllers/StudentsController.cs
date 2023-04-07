using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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


            //var list = new List<StudentPayment>();
            var req2 = new GetSchedulesByUserId(user.UserId.ToString());
            var res2 = _requestService.SendGet(req2, HttpContext);
            var schedules = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Schedule>>(res2.result.ToString());

            var startDate = DateTime.Parse("20.03.2023 12:00");
            user.BalanceHistory.Reverse();
            var keys = new Dictionary<DateTime, List<StudentPayment>>();
            var balanceHistories = user.BalanceHistory.GroupBy(x => x.Date.ToShortDateString()).ToList();
            for (int i = 0; i < balanceHistories.Count; i++)
            {
                balanceHistories[i].OrderBy(m => m.Date).Reverse();
                
            }
            for (int i = 0; i < balanceHistories.Count; i++)
            {
                var list2 = balanceHistories[i].ToList();
                for (int j = 0; j < list2.Count; j++)
                {
                    if (list2[j].CashFlow == null)
                    {
                        continue;
                    }
                    var date = balanceHistories[i].FirstOrDefault().Date;
                    if (date < startDate)
                    {
                        continue;
                    }

                    DateTime date2 = DateTime.MaxValue;
                    if (balanceHistories.Count > 1)
                    {
                        if ((i + 1) == balanceHistories.Count)
                        {
                            date2 = DateTime.MaxValue;
                        }
                        else
                        {

                            date2 = balanceHistories[i + 1].FirstOrDefault().Date;
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
                                if (ready > date2 && (date2 != DateTime.MaxValue))
                                {
                                    continue;
                                }
                                if (ready >= date && ready <= date2)
                                {

                                    if (keys.Count == 0)
                                    {

                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = list2[j].CashFlow.Amount/list2[j].CashFlow.Count,
                                            PaymentDate = date,
                                            PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        } });
                                    }
                                    else
                                    {
                                        if (keys.ContainsKey(date))
                                        {
                                            keys[date].Add(new StudentPayment()
                                            {
                                                LessonAmount = list2[j].CashFlow.Amount / list2[j].CashFlow.Count,
                                                StudentName = item.UserName,
                                                LessonDate = ready
                                            });
                                        }
                                        else
                                        {
                                            keys.Add(date, new List<StudentPayment>() { new StudentPayment() { LessonAmount = 1000, PaymentDate = date, PaymentAmount = list2[j].CashFlow.Amount, StudentName = item.UserName, LessonDate = ready } });
                                        }

                                    }
                                }
                                else if (ready >= date2 && (date2 != DateTime.MinValue))
                                {
                                    if(keys.ContainsKey(date))
                                    {
                                        keys[date].Add(new StudentPayment()
                                        {
                                            LessonAmount = list2[j].CashFlow.Amount / list2[j].CashFlow.Count,

                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        });
                                    }
                                    else
                                    {
                                        keys.Add( date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = list2[j].CashFlow.Amount/list2[j].CashFlow.Count,
                                            PaymentDate = date,
                                            PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        } });
                                    }
                            
                                }
                            }

                            foreach (var skiped in item.SkippedDates)
                            {
                                var ready = skiped.Date;
                                var credit = user.Credit.FirstOrDefault(m => m.ScheduleId == item.Id && m.ScheduleSkippedDate == ready);
                                if (credit == null)
                                {
                                    continue;
                                }
                                if (item.SkippedDates[0].WasWarn == true)
                                {
                                    continue;
                                }
                                if (ready > date2 && (date2 != DateTime.MaxValue))
                                {
                                    continue;
                                }

                                if (credit == null)
                                {
                                    if (keys.ContainsKey(date))
                                    {
                                        if (keys[date].FirstOrDefault(m=>m.LessonDate == ready) != null)
                                        {
                                            continue;
                                        }

                                        keys[date].Add(new StudentPayment()
                                        {
                                            LessonAmount = item.SkippedDates[0].InitPaid,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        });
                                    }
                                    else
                                    {
                                        
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = item.SkippedDates[0].InitPaid,
                                            PaymentDate = date,
                                            PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        }});
                                    }

                                    continue;
                                }

                                var paid = credit.Repaid;
                                var amount = credit.Amount;

                                if (ready >= date && ready < date2)
                                {
                                    if (keys.Count == 0)
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount =  paid == true ? (int)Math.Abs(amount) : 0,
                                            PaymentDate = date,
                                            PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        } });
                                    }
                                    else
                                    {
                                        if (keys.ContainsKey(date))
                                        {
                                            if (keys[date].FirstOrDefault(m => m.LessonDate == ready) != null)
                                            {
                                                continue;
                                            }
                                            keys[date].Add(new StudentPayment()
                                            {
                                                LessonAmount = paid == true ? (int)Math.Abs(amount) : 0,

                                                StudentName = item.UserName,
                                                LessonDate = ready
                                            });
                                        }
                                        else
                                        {
                                            keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount =  paid == true ? (int)Math.Abs(amount) : 0,
                                             PaymentDate = date,
                                        PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        } });
                                        }


                                    }
                                }
                                else if (ready >= date2 && (date2 != DateTime.MinValue))
                                {
                                    if (keys.ContainsKey(date))
                                    {if (keys[date].FirstOrDefault(m => m.LessonDate == ready) != null)
                                        {
                                            continue;
                                        }
                                        keys[date].Add(new StudentPayment()
                                        {
                                            LessonAmount = paid == true ? (int)Math.Abs(amount) : 0,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        });
                                    }
                                    else
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = paid == true ? (int)Math.Abs(amount) : 0,
                                            PaymentDate = date,
                                            PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        }});
                                    }

                                }
                            }
                        }
                        else
                        {

                            if (item.Status == Status.Пропущен)
                            {
                                var ready = item.SkippedDates[0].Date;
                                var credit = user.Credit.FirstOrDefault(m => m.ScheduleId == item.Id && m.ScheduleSkippedDate == ready);
                                
                               
                                if (item.SkippedDates[0].WasWarn == true)
                                {
                                    continue;
                                }

                                if(credit == null)
                                {
                                    if (keys.ContainsKey(date))
                                    {if (keys[date].FirstOrDefault(m => m.LessonDate == ready) != null)
                                        {
                                            continue;
                                        }
                                        keys[date].Add(new StudentPayment()
                                        {
                                            LessonAmount = item.SkippedDates[0].InitPaid,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        });
                                    }
                                    else
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = item.SkippedDates[0].InitPaid,
                                            PaymentDate = date,
                                            PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        }});
                                    }

                                    continue;
                                }

                                if (ready > date2 && (date2 != DateTime.MaxValue))
                                {
                                    continue;
                                }



                                var paid = credit.Repaid;
                                var amount = credit.Amount;

                                if (ready >= date && ready < date2)
                                {
                                    if (keys.Count == 0)
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount =  paid == true ? (int)Math.Abs(amount) : 0,
                                            PaymentDate = date,
                                            PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        } });
                                    }
                                    else
                                    {
                                        if (keys.ContainsKey(date))
                                        {if (keys[date].FirstOrDefault(m => m.LessonDate == ready) != null)
                                            {
                                                continue;
                                            }
                                            keys[date].Add(new StudentPayment()
                                            {
                                                LessonAmount = paid == true ? (int)Math.Abs(amount) : 0,

                                                StudentName = item.UserName,
                                                LessonDate = ready
                                            });
                                        }
                                        else
                                        {
                                            keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount =  paid == true ? (int)Math.Abs(amount) : 0,
                                             PaymentDate = date,
                                        PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        } });
                                        }


                                    }
                                }
                                else if (ready >= date2 && (date2 != DateTime.MinValue))
                                {
                                    if (keys.ContainsKey(date))
                                    {if (keys[date].FirstOrDefault(m => m.LessonDate == ready) != null)
                                        {
                                            continue;
                                        }
                                        keys[date].Add(new StudentPayment()
                                        {
                                            LessonAmount = paid == true ? (int)Math.Abs(amount) : 0,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        });
                                    }
                                    else
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                    {
                                        LessonAmount = paid == true ? (int)Math.Abs(amount) : 0,
                                        PaymentDate = date,
                                        PaymentAmount = list2[j].CashFlow.Amount,
                                        StudentName = item.UserName,
                                        LessonDate = ready
                                    }});
                                    }

                                }

                            }


                            if (item.Status == Status.Проведен)
                            {
                                var ready = item.EndDate;
                                if(ready > date2 && (date2 != DateTime.MaxValue))
                                {
                                    continue;
                                }
                                if (ready >= date && ready < date2)
                                {
                                    if (keys.Count == 0)
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = list2[j].CashFlow.Amount/list2[j].CashFlow.Count,
                                            PaymentDate = date,
                                            PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        } });
                                    }
                                    else
                                    {
                                        if (keys.ContainsKey(date))
                                        {if (keys[date].FirstOrDefault(m => m.LessonDate == ready) != null)
                                            {
                                                continue;
                                            }
                                            keys[date].Add(new StudentPayment()
                                            {
                                                LessonAmount = list2[j].CashFlow.Amount / list2[j].CashFlow.Count,

                                                StudentName = item.UserName,
                                                LessonDate = ready
                                            });
                                        }
                                        else
                                        {
                                            keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = list2[j].CashFlow.Amount / list2[j].CashFlow.Count,
                                             PaymentDate = date,
                                        PaymentAmount = list2[j].CashFlow.Amount,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        } });
                                        }
                                       

                                    }
                                }
                                else if (ready >= date2 && (date2 != DateTime.MinValue))
                                {
                                    if (keys.ContainsKey(date))
                                    {if (keys[date].FirstOrDefault(m => m.LessonDate == ready) != null)
                                        {
                                            continue;
                                        }
                                        keys[date].Add(new StudentPayment()
                                        {
                                            LessonAmount = list2[j].CashFlow.Amount / list2[j].CashFlow.Count,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        });
                                    }
                                    else
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                    {
                                        LessonAmount = list2[j].CashFlow.Amount / list2[j].CashFlow.Count,
                                        PaymentDate = date,
                                        PaymentAmount = list2[j].CashFlow.Amount,
                                        StudentName = item.UserName,
                                        LessonDate = ready
                                    }});
                                    }
                                  
                                }

                            }
                        }
                    }
                }

            }

            return View(keys);
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

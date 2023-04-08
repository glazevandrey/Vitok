using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using web_app.Models;
using web_server.DbContext;
using web_server.Models;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class StatisticsService : IStatisticsService
    {
        public Dictionary<DateTime, List<StudentPayment>> FormingStatData(string args)
        {

            var user = TestData.UserList.FirstOrDefault(m=>m.UserId == Convert.ToInt32(args));
            var schedules = TestData.Schedules.Where(m => m.UserId == user.UserId);

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
                var payments = list2.Where(m => m.CashFlow != null);

                string paymentAmount = "";
                foreach (var item in payments)
                {
                    paymentAmount += item.CashFlow.Amount + " + ";
                }
                paymentAmount = paymentAmount.Trim().TrimEnd('+').Trim();

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
                                            LessonAmount = item.PaidLessons[ready],
                                            PaymentDate = date,
                                            PaymentAmount = paymentAmount,
                                            StudentName = item.UserName,
                                            LessonDate = ready,
                                            LessonLooped = item.Looped
                                        } });
                                    }
                                    else
                                    {
                                        if (keys.ContainsKey(date))
                                        {
                                            keys[date].Add(new StudentPayment()
                                            {
                                                LessonAmount = item.PaidLessons[ready],
                                                StudentName = item.UserName,
                                                LessonDate = ready,
                                                LessonLooped = item.Looped
                                            });
                                        }
                                        else
                                        {
                                            keys.Add(date, new List<StudentPayment>() { new StudentPayment() { LessonAmount = 1000, PaymentDate = date, PaymentAmount = paymentAmount, StudentName = item.UserName, LessonDate = ready } });
                                        }

                                    }
                                }
                                else if (ready >= date2 && (date2 != DateTime.MinValue))
                                {
                                    if (keys.ContainsKey(date))
                                    {
                                        keys[date].Add(new StudentPayment()
                                        {
                                            LessonAmount = item.PaidLessons[ready],
                                            LessonLooped = item.Looped,
                                            StudentName = item.UserName,
                                            LessonDate = ready
                                        });
                                    }
                                    else
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = item.PaidLessons[ready],
                                            PaymentDate = date,
                                            LessonLooped = item.Looped,
                                            PaymentAmount = paymentAmount,
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
                                        if (keys[date].FirstOrDefault(m => m.LessonDate == ready) != null)
                                        {
                                            continue;
                                        }

                                        keys[date].Add(new StudentPayment()
                                        {
                                            LessonAmount = item.SkippedDates[0].InitPaid,
                                            StudentName = item.UserName,
                                            LessonLooped = item.Looped,
                                            LessonDate = ready
                                        });
                                    }
                                    else
                                    {

                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = item.SkippedDates[0].InitPaid,
                                            PaymentDate = date,
                                            PaymentAmount = paymentAmount,
                                            StudentName = item.UserName,
                                            LessonLooped = item.Looped,
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
                                            PaymentAmount = paymentAmount,
                                            StudentName = item.UserName,
                                            LessonLooped = item.Looped,
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
                                                LessonLooped = item.Looped,
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
                                        PaymentAmount = paymentAmount,
                                            StudentName = item.UserName,
                                            LessonDate = ready,
                                            LessonLooped = item.Looped
                                        } });
                                        }


                                    }
                                }
                                else if (ready >= date2 && (date2 != DateTime.MinValue))
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
                                            LessonDate = ready,
                                            LessonLooped = item.Looped
                                        });
                                    }
                                    else
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = paid == true ? (int)Math.Abs(amount) : 0,
                                            PaymentDate = date,
                                            PaymentAmount = paymentAmount,
                                            StudentName = item.UserName,
                                            LessonDate = ready,
                                            LessonLooped = item.Looped
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

                                if (credit == null)
                                {
                                    if (keys.ContainsKey(date))
                                    {
                                        if (keys[date].FirstOrDefault(m => m.LessonDate == ready) != null)
                                        {
                                            continue;
                                        }
                                        keys[date].Add(new StudentPayment()
                                        {
                                            LessonAmount = item.SkippedDates[0].InitPaid,
                                            StudentName = item.UserName,
                                            LessonDate = ready,
                                            LessonLooped = item.Looped
                                        });
                                    }
                                    else
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = item.SkippedDates[0].InitPaid,
                                            PaymentDate = date,
                                            PaymentAmount = paymentAmount,
                                            StudentName = item.UserName,
                                            LessonDate = ready,
                                            LessonLooped = item.Looped
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
                                            PaymentAmount = paymentAmount,
                                            StudentName = item.UserName,
                                            LessonDate = ready,
                                            LessonLooped = item.Looped
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
                                                LessonLooped = item.Looped,
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
                                        PaymentAmount = paymentAmount,
                                            StudentName = item.UserName,
                                            LessonDate = ready,
                                            LessonLooped = item.Looped
                                        } });
                                        }


                                    }
                                }
                                else if (ready >= date2 && (date2 != DateTime.MinValue))
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
                                            LessonDate = ready,
                                            LessonLooped = item.Looped
                                        });
                                    }
                                    else
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                    {
                                        LessonAmount = paid == true ? (int)Math.Abs(amount) : 0,
                                        PaymentDate = date,
                                        PaymentAmount = paymentAmount,
                                        StudentName = item.UserName,
                                        LessonDate = ready,
                                        LessonLooped = item.Looped
                                    }});
                                    }

                                }

                            }


                            if (item.Status == Status.Проведен)
                            {
                                var ready = item.EndDate;
                                if (ready > date2 && (date2 != DateTime.MaxValue))
                                {
                                    continue;
                                }
                                if (ready >= date && ready < date2)
                                {
                                    if (keys.Count == 0)
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = item.PaidLessons[ready],
                                            PaymentDate = date,
                                            PaymentAmount = paymentAmount,
                                            StudentName = item.UserName,
                                            LessonDate = ready,
                                            LessonLooped = item.Looped
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
                                                LessonAmount = item.PaidLessons[ready],

                                                StudentName = item.UserName,
                                                LessonDate = ready,
                                                LessonLooped = item.Looped
                                            });
                                        }
                                        else
                                        {
                                            keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                        {
                                            LessonAmount = item.PaidLessons[ready],
                                             PaymentDate = date,
                                        PaymentAmount = paymentAmount,
                                            StudentName = item.UserName,
                                            LessonDate = ready,
                                            LessonLooped = item.Looped
                                        } });
                                        }


                                    }
                                }
                                else if (ready >= date2 && (date2 != DateTime.MinValue))
                                {
                                    if (keys.ContainsKey(date))
                                    {
                                        if (keys[date].FirstOrDefault(m => m.LessonDate == ready) != null)
                                        {
                                            continue;
                                        }
                                        keys[date].Add(new StudentPayment()
                                        {
                                            LessonAmount = item.PaidLessons[ready],
                                            StudentName = item.UserName,
                                            LessonDate = ready,
                                            LessonLooped = item.Looped
                                        });
                                    }
                                    else
                                    {
                                        keys.Add(date, new List<StudentPayment>(){ new StudentPayment()
                                    {
                                            LessonAmount = item.PaidLessons[ready],
                                        PaymentDate = date,
                                        PaymentAmount = paymentAmount,
                                        StudentName = item.UserName,
                                        LessonDate = ready,
                                        LessonLooped = item.Looped
                                    }});
                                    }

                                }

                            }
                        }
                    }
                }

            }

            var hh = new Dictionary<DateTime, List<StudentPayment>>(keys);



            foreach (var item in keys)
            {
                var ordered = item.Value.OrderBy(m => m.LessonDate).ToList();
                var first = ordered.FirstOrDefault();

                if (first.PaymentDate == DateTime.MinValue) // Если первый элемент имеет PaymentAmount = "0"
                {
                    var next = ordered.FirstOrDefault(m => m.PaymentDate != DateTime.MinValue);
                    
                    if (next != null) // Если следующий элемент существует
                    {
                        first.PaymentAmount = next.PaymentAmount; // Изменить PaymentAmount первого элемента
                        first.PaymentDate = next.PaymentDate; // Изменить PaymentDate первого элемента
                        next.PaymentAmount = null; // Установить PaymentAmount следующего элемента в "0"
                        next.PaymentDate = DateTime.MinValue; // Установить PaymentDate следующего элемента в минимальное значение
                    }
                }

                hh[item.Key] = ordered.ToList();
            }
            

            return hh;
        }
    }
}

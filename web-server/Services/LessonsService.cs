using System;
using System.Collections.Generic;
using System.Linq;
using web_server.DbContext;
using web_server.Models;

namespace web_server.Services
{
    public class LessonsService : ILessonsService
    {
        public List<RescheduledLessons> GetRescheduledLessons(string args)
        {
            var user = TestData.UserList.FirstOrDefault(m => m.ActiveToken == args);
            if (user == null)
            {
                return null;
            }

            var schedules = new List<RescheduledLessons>();
            if (user.Role == "Tutor")
            {
                schedules = TestData.RescheduledLessons.Where(m => m.TutorId == user.UserId).ToList();
            }
            else
            {
                schedules = TestData.RescheduledLessons.Where(m => m.UserId == user.UserId).ToList();
            }

            if (schedules == null || schedules.Count == 0)
            {
                schedules = TestData.RescheduledLessons.Where(m => m.TutorId == user.UserId).ToList();
            }

            return schedules;
        }

        public User AddLessonsToUser(string[] args)
        {

            var user = TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(args[0]));
            user.LessonsCount += Convert.ToInt32(args[1]);

            var schedules = TestData.Schedules.Where(m => m.UserId == user.UserId).ToList();
            var isTrial = Convert.ToBoolean(args[2]);
            if (isTrial)
            {
                TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).UsedTrial = true;
            }
            var lessonCount = Convert.ToInt32(args[1]);
            var tariff = TestData.Tariffs.FirstOrDefault(m => m.LessonsCount == lessonCount);
            if (tariff != null)
            {
                TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).BalanceHistory.CustomMessages.Add(DateTime.Now, $"Оплата тарифа: {tariff.Title}");
            }
            else
            {
                TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId).BalanceHistory.CustomMessages.Add(DateTime.Now, $"Оплачено занятий: {lessonCount}");
            }

            for (int i = 0; i < Convert.ToInt32(args[1]); i++)
            {
                var waited = schedules.Where(m => m.Status == Status.ОжидаетОплату).ToList();
                if (waited.Count > 0)
                {
                    schedules.FirstOrDefault(m => m.Id == waited.First().Id).Status = Status.Ожидает;
                    user.LessonsCount -= 1;
                }
            }

            return user;
        }
    }
}

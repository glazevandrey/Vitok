using System.Collections.Generic;
using System.Linq;
using web_server.DbContext;
using web_server.Models;

namespace web_server.Services
{
    public class AccountService : IAccountService
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

        public List<Schedule> GetSchedules(string args)
        {
            var user = TestData.UserList.FirstOrDefault(m => m.ActiveToken == args);
            if (user == null)
            {
                return null;
            }

            var schedules = TestData.Schedules.Where(m => m.UserId == user.UserId).ToList();
            if (schedules == null || schedules.Count == 0)
            {
                schedules = TestData.Schedules.Where(m => m.TutorId == user.UserId).ToList();

            }
            return schedules;
        }

        public User SaveAccountInfo(string args)
        {
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(args);
            var old = TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId);

            old.FirstName = user.FirstName;
            old.LastName = user.LastName;
            old.BirthDate = user.BirthDate;
            old.About = user.About;
            old.Email = user.Email;
            old.Wish = user.Wish;
            old.Password = user.Password;
            old.Phone = user.Phone;

            var index = TestData.UserList.FindIndex(m => m.UserId == user.UserId);
            TestData.UserList[index] = old;

            return user;
        }
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using web_server.DbContext;
using web_server.Models;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class AccountService : IAccountService
    {
        public bool RemoveFirstLogin(string args)
        {
            TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(args)).FirstLogin = false;
            return true;
        }

        public User SaveAccountInfo(string args)
        {
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(args);
            var old = TestData.UserList.FirstOrDefault(m => m.UserId == user.UserId);

            var schedules = new List<Schedule>();
            if (user.Role == "Tutor")
            {
                schedules = TestData.Schedules.Where(m => m.TutorId == user.UserId).ToList();
            }
            else if (user.Role == "Student")
            {
                schedules = TestData.Schedules.Where(m => m.UserId == user.UserId).ToList();
            }
            foreach (var item in schedules)
            {
                var new_name = "";
                string[] name = null;

                if (user.Role == "Tutor")
                {
                    name = item.TutorFullName.Split(" ");
                }
                else
                {
                    name = item.UserName.Split(" ");
                }
                if (user.FirstName.Contains(" ") || user.LastName.Contains(" "))
                {
                    return null;
                }
                if (name[0] != user.FirstName)
                {
                    new_name = user.FirstName;
                }
                else
                {
                    new_name = name[0];
                }
                new_name += " ";
                if (name[1] != user.LastName)
                {
                    new_name += user.LastName;
                }
                else
                {
                    new_name += name[1];
                }
                if (user.Role == "Student")
                {
                    item.UserName = new_name;
                }
                else if (user.Role == "Tutor")
                {
                    item.TutorFullName = new_name;
                }
            }

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

        public string SavePhoto(IFormFile file, string id)
        {
            string imageName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/avatars", imageName);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(id)).PhotoUrl = "http://localhost:23382/" + "avatars/" + imageName;

            return savePath;
        }

        public bool Withdraw(string tutorid, string count)
        {
            if (TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutorid)).Balance < Convert.ToDouble(count))
            {
                return false;
            }
            TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutorid)).Balance -= Convert.ToInt32(count);
            TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(tutorid)).BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = Convert.ToInt32(count) }, CustomMessages = new CustomMessage() { MessageValue = $"Вывод средств" } });

            //.CustomMessages.Add(new CustomMessage() { MessageKey = DateTime.Now, MessageValue = $"Вывод средств: {count} p." });

            return true;
        }
    }
}

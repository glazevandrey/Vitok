using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Linq;
using web_server.DbContext;
using web_server.Models;

namespace web_server.Services
{
    public class AccountService : IAccountService
    {

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

        public string SavePhoto(IFormFile file, string id)
        {
            string imageName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/avatars", imageName);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            TestData.UserList.FirstOrDefault(m=>m.UserId == Convert.ToInt32(id)).PhotoUrl = "http://localhost:23382/" + "avatars/" + imageName;

            return savePath;
        }
    }
}

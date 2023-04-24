using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.Models.DBModels;
using web_server.Models.DTO;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class AccountService : IAccountService
    {
        UserRepository _userRepository;
        ScheduleRepository _scheduleRepository;
        public AccountService(UserRepository userRepository, ScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
            _userRepository = userRepository;
        }

        public async Task<List<User>> GetAllUserContacts(string id, string role)
        {
            List<User> res = new List<User>();
            if(role == "Tutor")
            {
                var schedules = await _scheduleRepository.GetSchedulesByFunc(m=>m.TutorId == Convert.ToInt32(id) && m.UserId != 1 && m.UserName != "");
                foreach (var item in schedules)
                {
                    if(res.FirstOrDefault(m=>m.UserId == item.UserId) == null)
                    {
                        res.Add(await _userRepository.GetLiteUser(item.UserId));
                    }
                }
            }
            else
            {
                var schedules = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == Convert.ToInt32(id));
                foreach (var item in schedules)
                {
                    if (res.FirstOrDefault(m => m.UserId == item.TutorId) == null)
                    {
                        res.Add(await _userRepository.GetLiteUser(item.TutorId));
                    }
                }
            }

            return res;
        }

        public async Task<bool> RemoveFirstLogin(string args)
        {
            var user = await _userRepository.GetStudent(Convert.ToInt32(args));
            user.FirstLogin = false;
            await _userRepository.SaveChanges(user);
            //TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(args)).FirstLogin = false;
            return true;
        }
        
        private async Task SaveMainInfo(UserDTO old, User user)
        {
            old.FirstName = user.FirstName;
            old.LastName = user.LastName;
            old.BirthDate = user.BirthDate;

            old.Email = user.Email;
            old.Password = user.Password;
            old.Phone = user.Phone;
        }
        public async Task<User> SaveAccountInfo(string args)
        {
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(args, Program.settings);
            
            StudentDTO student= new StudentDTO();
            TutorDTO tutor = new TutorDTO();
            UserDTO manager = new UserDTO();
            
            if(user.Role == "Tutor")
            {
                tutor = await _userRepository.GetTutor(user.UserId);
                await SaveMainInfo(tutor, user);
                tutor.About= ((Tutor)user).About;

                await _userRepository.SaveChanges(tutor);


            }
            else if (user.Role == "Student")
            {
                student = await _userRepository.GetStudent(user.UserId);
                await SaveMainInfo(student, user);
                student.Wish = ((Student)user).Wish;
                await _userRepository.SaveChanges(student);


            }
            else
            {
                manager = await _userRepository.GetUser(user.UserId);
                await SaveMainInfo(manager, user);
                await _userRepository.SaveChanges(manager);

            }


            var schedules = new List<ScheduleDTO>();
            if (user.Role == "Tutor")
            {
                schedules = await _scheduleRepository.GetSchedulesByFunc(m => m.TutorId == user.UserId);//TestData.Schedules.Where(m => m.TutorId == user.UserId).ToList();
            }
            else if (user.Role == "Student")
            {
                schedules = await _scheduleRepository.GetSchedulesByFunc(m => m.UserId == user.UserId);//TestData.Schedules.Where(m => m.UserId == user.UserId).ToList();
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
            await _scheduleRepository.UpdateRange(schedules);


            //var index = TestData.UserList.FindIndex(m => m.UserId == user.UserId);
            //TestData.UserList[index] = old;

            return user;
        }

        public async Task<string> SavePhoto(IFormFile file, string id)
        {
            string imageName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/avatars", imageName);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var user = await _userRepository.GetUserById(Convert.ToInt32(id));
            user.PhotoUrl = "http://localhost:23382/" + "avatars/" + imageName;
            await _userRepository.Update(user);
            //TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(id)).PhotoUrl = "http://localhost:23382/" + "avatars/" + imageName;

            return savePath;
        }

        public async Task<bool> Withdraw(string tutorid, string count)
        {
            var user = await _userRepository.GetUser(Convert.ToInt32(tutorid));
            if (user.Balance < Convert.ToDouble(count))
            {
                return false;
            }

            user.Balance -= Convert.ToInt32(count);
            user.BalanceHistory.Add(new BalanceHistory() { CashFlow = new CashFlow() { Amount = Convert.ToInt32(count) }, CustomMessage = $"Вывод средств" });

            await _userRepository.SaveChanges(user);
            //.CustomMessages.Add(new CustomMessage() { MessageKey = DateTime.Now, MessageValue = $"Вывод средств: {count} p." });

            return true;
        }
    }
}

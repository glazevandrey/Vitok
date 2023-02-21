using web_server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

namespace web_server.DbContext
{
    public static class TestData
    {
        public static List<Goal> Goals { get; set; } = new List<Goal>() { new Goal() { Id = 0, Title = "Сдать экзамен" }, new Goal() { Id = 1, Title = "Другое" } };
        public static List<Course> Courses { get; set; } = new List<Course>() {
            new Course() {Id = 0, Title="ОГЭ" ,Goal = Goals.FirstOrDefault(m=>m.Title == "Сдать экзамен") },
            new Course() {Id = 1, Title="ЕГЭ" ,Goal = Goals.FirstOrDefault(m=>m.Title == "Сдать экзамен") },
            new Course() {Id = 2, Title="Общий английский" ,Goal = Goals.FirstOrDefault(m=>m.Title == "Другое") }
        };
        
        public static List<SiteContact> Contacts = new List<SiteContact> { new SiteContact() { Title = "Телефон", Text = "+790523232" }, new SiteContact { Title = "Телеграм", Text = "@andreyglazev" } };
        public static List<InChat> LiveChats { get; set; } = new List<InChat>();
        public static List<Registration> Registations = new List<Registration>();
        public static List<RescheduledLessons> RescheduledLessons = new List<RescheduledLessons>();
        public static List<User> UserList = new List<User>
            {
             new User() {FirstName = "Сергей", LastName = "Петров", About = "Лучший", BirthDate = DateTime.Parse("15.02.2001"),
                    Courses = TestData.Courses.Where(m => m.Title == "Общий английский").ToList(), UserId =0, Email = "b", Phone = "+79054769537",
                    PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg", Password = "123", Role="Tutor", UserDates= new UserDate(){
                        dateTimes = new List<DateTime>(){DateTime.Parse("10.02.2023 17:00"), DateTime.Parse("16.02.2023 19:00") }
                    }},

                new User() {FirstName = "Иван", LastName = "Петров", About = "Почти лучший", BirthDate = DateTime.Parse("14.01.2002"),
                    Courses = Courses.Where(m => m.Title == "ОГЭ").ToList(), UserId=1, Email = "a.glazev@mail.ru", Phone = "+79188703839",
                    PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg", Password = "123", Role="Tutor", UserDates= new UserDate(){
                        dateTimes = new List<DateTime>(){DateTime.Parse("10.02.2023 15:00"), DateTime.Parse("13.02.2023 17:00") }
                    }},
 new User
                {
                    FirstName = "Петр",
                    LastName = "Иванов",
                    Password = "448",
                    Role = "Student",
                    Email = "a@mail.ru",
                    Phone = "+79188793839",
                    UserId = 3
                },
                  new User
                {
                    FirstName = "Сергей",
                    LastName = "Курочка",
                    Password = "123123",
                    Role = "Student",
                    Email = "a",
                    Phone = "+79188793839",
                    UserId = 4
                }
            };

        public static List<User> Tutors = UserList.Where(m => m.Role == "Tutor").ToList();

        public static List<Schedule> Schedules = new List<Schedule>() { new Schedule() {Id = 2, UserId=-1, TutorFullName = "Сергей Петров", TutorId = 0, Date = new UserDate(){ dateTimes = new List<DateTime>(){ DateTime.Parse("16.02.2023 19:00") } } },
            new Schedule() { Id = 0, UserName = "Andrey", TutorFullName = "Иван Петров", TutorId = 1, UserId = 4,
                Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("09.02.2023 23:00") } } },
            new Schedule() { Id = 1, UserName ="Andrey", TutorFullName = "Сергей Петров", TutorId = 0, UserId = 4,
                Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("10.02.2023 20:00") } }},
          new Schedule() { Id = 3, UserName ="Сергей", TutorFullName = "Сергей Петров", TutorId = 0, UserId = 4,
                Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("23.02.2023 13:00") } }, Looped = false},
         new Schedule() { Id = 4, UserName ="Сергей", TutorFullName = "Сергей Петров", TutorId = 0, UserId = 4,
                Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("23.02.2023 17:00") } }, Looped = false}};

        public static List<ChatUser> ChatUsers { get; set; } = new List<ChatUser>();
    }
    public class RescheduledLessons
    {
        public int UserId { get; set; }
        public int TutorId { get; set; }
        public DateTime OldTime { get; set; }
        public DateTime NewTime { get; set; }
    }
    public class ChatUser
    {
        public int UserId { get; set; }
        public Dictionary<string, string> ConnectionTokens { get; set; }
        public List<Messages> Messages { get; set; } = new List<Messages>();
        public List<Contact> Contacts { get; set; } = new List<Contact>();
    }
    public class Contact
    {
        public string DisplayName{ get; set; }
        public int UserId{ get; set; }
    }

    public class Messages
    {
        public DateTime MessageTime { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set;}
        public string Message { get; set; }
    }

    public class InChat 
    {
        public string UserId { get; set; }
        public string WithUserId { get; set; }
    }
}

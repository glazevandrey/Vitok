using System;
using System.Collections.Generic;
using System.Linq;
using web_server.Models;
using web_server.Models.DBModels;

namespace web_server.DbContext
{
    public static class TestData
    {
        public static List<Goal> Goals { get; set; } = new List<Goal>() { new Goal() { Id = 0, Title = "Сдать экзамен" },
            new Goal() { Id = 1, Title = "Изучить с нуля" }, new Goal() { Id = 2, Title = "Работать" }, new Goal() { Id = 3, Title = "Преодолеть языковой барьер" },
            new Goal() { Id = 4, Title = "Проходить собеседование" }, new Goal() { Id = 5, Title = "Путешествовать" }, new Goal() { Id = 6, Title = "Повысить уровень" },
            };

        public static List<Course> Courses { get; set; } = new List<Course>() {
            new Course() {Id = 0, Title="ОГЭ" ,Goals = Goals.Where(m=>m.Title == "Сдать экзамен").ToList() },
            new Course() {Id = 1, Title="ЕГЭ" ,Goals = Goals.Where(m=>m.Title == "Сдать экзамен").ToList() },
            new Course() {Id = 2, Title="Общий английский" ,Goals = Goals.Where(m=>m.Id >= 1).ToList() }
        };

        public static List<Tariff> Tariffs = new List<Tariff>()
        {
            new Tariff() { Amount = 2700, Id = 0, LessonsCount = 3, Title = "ПАКЕТ занятий 3"},
            new Tariff() { Amount = 7650, Id = 1, LessonsCount = 9, Title = "ПАКЕТ занятий 9"},
            new Tariff() { Amount = 21600, Id = 2, LessonsCount = 27, Title = "ПАКЕТ занятий 27"},
            new Tariff() { Amount = 56700, Id = 3, LessonsCount = 81, Title = "ВЫГОДНЫЙ ПАКЕТ ЗАНЯТИЙ 81"}
        };

        public static List<SiteContact> Contacts = new List<SiteContact> { new SiteContact() { Title = "Телефон", Text = "+790523232" }, new SiteContact { Title = "Телеграм", Text = "@andreyglazev" } };
        public static List<InChat> LiveChats { get; set; } = new List<InChat>();
        public static List<Registration> Registations = new List<Registration>();
        public static List<RescheduledLessons> RescheduledLessons = new List<RescheduledLessons>();

        // все пользователи
        public static List<User> UserList = new List<User>
            {
             new User() {FirstName = "Сергей", LastName = "Глузер",MiddleName="Сергеевич", About = "Лучший", BirthDate = DateTime.Parse("15.02.2001"),
                    Courses = TestData.Courses.Where(m => m.Title == "Общий английский").ToList(), UserId =0,WasFirstPayment = true, Email = "sergey@mail.ru", Phone = "+79054769537",
                    PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg", Password = "123", Role="Tutor", UserDates= new UserDate(){
                        dateTimes = new List<DateTime>(){ }
                    }},

                new User() {FirstName = "Иван", MiddleName="Сергеевич", LastName = "Петров", About = "Почти лучший", BirthDate = DateTime.Parse("14.01.2002"),
                    Courses = Courses.Where(m => m.Title == "ОГЭ").ToList(), UserId=1,WasFirstPayment = true, Email = "ivan@mail.ru", Phone = "+79188703839",
                    PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg", Password = "123", Role="Tutor", UserDates= new UserDate(){
                        dateTimes = new List<DateTime>(){ }
                    }},
                new User
                {
                    FirstName = "Петр",
                    MiddleName = "Андреевич",
                    LastName = "Иванов",
                    Password = "123",
                    Role = "Student",
                    WasFirstPayment = true,
                    StartWaitPayment = DateTime.Now,
                     PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg",
                    Email = "petr@mail.ru",
                    BirthDate = DateTime.Parse("14.04.2000"),
                    Phone = "+79188793839",
                    UserId = 3
                },
                new User
                {
                    FirstName = "Сергей",
                    LastName = "Курочка",
                    MiddleName = "Андреевич",
                    LessonsCount = 1,
                    Money = new List<UserMoney>(){ new UserMoney() { Id = 0, Cost = 1000, Count = 1} },
                    BirthDate = DateTime.Parse("12.05.1999"),
                    Password = "123123",
                    WasFirstPayment = true,
                    Role = "Student",
                    Email = "kurochka@mail.ru",
                    PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg",
                    Phone = "+79188793839",
                    UserId = 4
                },
                  new User
                {
                    FirstName = "Андрей",
                    MiddleName = "Анатольевич",
                    LastName = "Глазев",
                    BirthDate = DateTime.Parse("02.03.1994"),
                    LessonsCount = 0,
                    Password = "123",
                    WasFirstPayment = true,
                     PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg",
                    Role = "Manager",
                    Email = "god@mail.ru",
                    Phone = "+79054769537",
                    UserId = 5
                }
            };
        public static List<Notifications> Notifications = new List<Notifications>();

        public static List<User> Tutors = UserList.Where(m => m.Role == "Tutor").ToList();
        public static List<User> Managers = UserList.Where(m => m.Role == "Manager").ToList();

        public static List<Schedule> Schedules = new List<Schedule>() {
            new Schedule() { Id = 0, UserName = "Петр Иванов", TutorFullName = "Иван Петров", TutorId = 1, UserId = 3,

                StartDate = DateTime.Parse("19.04.2023 23:00"), WaitPaymentDate = DateTime.Parse("19.04.2023 23:00"), Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("09.04.2023 23:00") } }, Course = Courses[0] },


            new Schedule() { Id = 1, UserName ="Петр Иванов", TutorFullName = "Сергей Глузер", TutorId = 0, UserId = 3,
             StartDate =    DateTime.Parse("10.04.2023 20:00"), WaitPaymentDate =  DateTime.Parse("10.04.2023 20:00"), Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("10.04.2023 20:00") } }, Course = Courses[1]},


            new Schedule() { Id = 2, UserName ="Сергей Курочка", TutorFullName = "Сергей Глузер", TutorId = 0, UserId = 4,
            StartDate = DateTime.Parse("26.04.2023 13:00"),
                Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("26.04.2023 13:00") } }, Looped = true, Course = Courses[2]},

            new Schedule() { Id = 3, UserName ="Сергей Курочка",TutorFullName = "Иван Петров", TutorId = 1, UserId = 4,
            StartDate = DateTime.Parse("08.04.2023 20:00"),
                Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("08.04.2023 20:00") } }, Looped = false, Course = Courses[0]},

            //new Schedule() { Id = 4, UserName ="Сергей Курочка",TutorFullName = "Иван Петров", TutorId = 1, UserId = 4,
            //    StartDate=DateTime.Parse("17.04.2023 7:00"), Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("17.04.2023 7:00") } }, Looped = true, Course = Courses[1]},

            new Schedule() { Id = 5, UserName ="Сергей Курочка",TutorFullName = "Сергей Глузер", TutorId = 0, UserId = 4,
                 StartDate = DateTime.Parse("17.04.2023 7:00"),
                Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("17.04.2023 7:00") } }, Looped = true, Course = Courses[1]},

            new Schedule() { Id = 6, UserName ="Сергей Курочка", TutorFullName = "Сергей Глузер", TutorId = 0, UserId = 4,
                StartDate = DateTime.Parse("13.04.2023 17:00"), CreatedDate = DateTime.Now,
                Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("13.04.2023 17:00") } }, Looped = true, Course = Courses[2]}

        };

        public static List<ChatUser> ChatUsers { get; set; } = new List<ChatUser>();
    }
}

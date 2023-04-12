using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DBModels.DTO;
using web_server.Models.DTO;

namespace web_server.DbContext
{
    public static class TestData
    {
        //public static List<Goal> Goals { get; set; } = new List<Goal>() { new Goal() { Id = 0, Title = "sd"} };

        public static List<Goal> Goals { get; set; } = new List<Goal>() {  new Goal() { Title = "Сдать экзамен" },
            new Goal() {Title = "Изучить с нуля" }, new Goal() {  Title = "Работать" }, new Goal() {  Title = "Преодолеть языковой барьер" },
            new Goal() {Title = "Проходить собеседование" }, new Goal() {  Title = "Путешествовать" }, new Goal() { Title = "Повысить уровень" },
            };

        public static List<CourseDTO> Courses { get; set; } = new List<CourseDTO>() {
            new CourseDTO() {Title="ОГЭ", Goal = Goals[0] ,},
            new CourseDTO() { Title="ЕГЭ", Goal = Goals[1], },
            new CourseDTO() {Title="Общий английский", Goal =Goals[2] }
        };

        public static List<Tariff> Tariffs = new List<Tariff>()
        {
            new Tariff() {  Amount = 2700,  LessonsCount = 3, Title = "ПАКЕТ занятий 3"},
            new Tariff() { Amount = 7650, LessonsCount = 9, Title = "ПАКЕТ занятий 9"},
            new Tariff() { Amount = 21600, LessonsCount = 27, Title = "ПАКЕТ занятий 27"},
            new Tariff() { Amount = 56700, LessonsCount = 81, Title = "ВЫГОДНЫЙ ПАКЕТ ЗАНЯТИЙ 81"}
        };

        public static List<SiteContact> Contacts = new List<SiteContact> { new SiteContact() {  Title = "Телефон", Text = "+790523232" }, new SiteContact { Title = "Телеграм", Text = "@andreyglazev" } };
        public static List<InChat> LiveChats { get; set; } = new List<InChat>();
        public static List<Registration> Registations = new List<Registration>();
        public static List<RescheduledLessons> RescheduledLessons = new List<RescheduledLessons>();

        public static List<Notifications> Notifications = new List<Notifications>();
        public static List<ScheduleDTO> Schedules = new List<ScheduleDTO>() {
            new ScheduleDTO() { UserName = "Петр Иванов", TutorFullName = "Иван Петров", TutorId = 5, UserId = 2,

                StartDate = DateTime.Parse("19.04.2023 23:00"), WaitPaymentDate = DateTime.Parse("19.04.2023 23:00"), Course = Courses[0] },


            //new Schedule() { UserName ="Петр Иванов", TutorFullName = "Сергей Глузер", TutorId = 6, UserId = 3,
            // StartDate =    DateTime.Parse("10.04.2023 20:00"), WaitPaymentDate =  DateTime.Parse("10.04.2023 20:00"), Date = new UserDate() { dateTime = DateTime.Parse("10.04.2023 20:00") }, Course = Courses[1]},


            //new Schedule() { UserName ="Сергей Курочка", TutorFullName = "Сергей Глузер", TutorId = 6, UserId =2,
            //StartDate = DateTime.Parse("26.04.2023 13:00"),
            //    Date = new UserDate(){dateTime =  DateTime.Parse("26.04.2023 13:00") } , Looped = true, Course = Courses[2]},

            //new Schedule() { UserName ="Сергей Курочка",TutorFullName = "Иван Петров", TutorId = 5, UserId = 2,
            //StartDate = DateTime.Parse("08.04.2023 20:00"),
            //    Date = new UserDate() { dateTime = DateTime.Parse("08.04.2023 20:00") }, Looped = false, Course = Courses[0]},

            ////new Schedule() { Id = 4, UserName ="Сергей Курочка",TutorFullName = "Иван Петров", TutorId = 1, UserId = 4,
            ////    StartDate=DateTime.Parse("17.04.2023 7:00"), Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("17.04.2023 7:00") } }, Looped = true, Course = Courses[1]},

            //new Schedule() { UserName ="Сергей Курочка",TutorFullName = "Сергей Глузер", TutorId = 6, UserId = 2,
            //     StartDate = DateTime.Parse("17.04.2023 7:00"),
            //    Date = new UserDate() { dateTime = DateTime.Parse("17.04.2023 7:00") }, Looped = true, Course = Courses[1]},

            //new Schedule() { UserName ="Сергей Курочка", TutorFullName = "Сергей Глузер", TutorId = 6, UserId = 2,
            //    StartDate = DateTime.Parse("13.04.2023 17:00"), CreatedDate = DateTime.Now,
            //    Date = new UserDate() { dateTime = DateTime.Parse("13.04.2023 17:00") }, Looped = true, Course = Courses[2]}

        };
        private static List<ChatUser> ChatUsers { get; set; } = new List<ChatUser>();

        public static List<UserDTO> UserList = new List<UserDTO>()
        {
                     new StudentDTO()
  {
                    FirstName = "Свободное",
                    MiddleName = "Андреевич",
                    LastName = "Занятие",
                    Password = "123",
                    Role = "Student",
                    Email = "petr@mail.ru",
                    BirthDate = DateTime.Parse("14.04.2000"),
                    Phone = "+79188793839",

                    //UserId = 3
                },
            new StudentDTO()
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
                    //UserId = 3
                },
            new StudentDTO()

           {
                    FirstName = "Сергей",
                    LastName = "Курочка",
                    MiddleName = "Андреевич",
                    LessonsCount = 1,
                    //Money = new List<UserMoney>(){ new UserMoney() { Id = 0, Cost = 1000, Count = 1} },
                    BirthDate = DateTime.Parse("12.05.1999"),
                    Password = "123123",
                    WasFirstPayment = true,
                    Role = "Student",
                    Email = "kurochka@mail.ru",
                    PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg",
                    Phone = "+79188793839",
                    //UserId = 4
                },
            new TutorDTO()
          {FirstName = "Сергей", LastName = "Глузер",MiddleName="Сергеевич", About = "Лучший", BirthDate = DateTime.Parse("15.02.2001"), UserDates = new List<UserDate>(){},
                    Courses = TestData.Courses.Where(m => m.Title == "Общий английский").ToList(),  Email = "sergey@mail.ru", Phone = "+79054769537",
                    PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg", Password = "123", Role="Tutor" },
            new TutorDTO()
             {FirstName = "Иван", MiddleName="Сергеевич", LastName = "Петров", About = "Почти лучший", BirthDate = DateTime.Parse("14.01.2002"), UserDates = new List<UserDate>(){ },
                    Courses = Courses.Where(m => m.Title == "ОГЭ").ToList(), Email = "ivan@mail.ru", Phone = "+79188703839",
                    PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg", Password = "123", Role="Tutor" },
            new ManagerDTO()
           {
                    FirstName = "Андрей",
                    MiddleName = "Анатольевич",
                    LastName = "Глазев",
                    BirthDate = DateTime.Parse("02.03.1994"),
                    Password = "123",
                     PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg",
                    Role = "Manager",
                    Email = "god@mail.ru",
                    Phone = "+79054769537",
                }
        };
    }
}

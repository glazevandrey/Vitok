using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using web_server.Models;

namespace web_server.DbContext
{
    public static class Constants
    {
        #region Общие уведомления для репетитора и студента
        public const string NOTIF_START_LESSON = "У вас началось занятие!"; // в момент наступления занятия
        public const string NOTIF_TOMORROW_LESSON = "Завтра у вас занятие!"; // за день + на почту
        #endregion

        #region Уведомления Репетитора
        public const string NOTIF_DONT_FORGET_SET_STATUS = "Не забудьте отметить статус занятия!"; // окончание занятия
        public const string NOTIF_NEW_STUDENT_FOR_TUTOR = "У вас появился новый ученик {name}"; // новый ученик записался
        public const string NOTIF_NEW_LESSON_TUTOR = "У вас новое занятие с учеником {name} на {date}"; // новый ученик записался

        #endregion

        #region Уведомления Ученика
        public const string NOTIF_ONE_LESSON_LEFT = "У вас осталось одно занятие! Не забудьте пополнить баланс!"; // когда у ученика осталось 1 занятие
        public const string NOTIF_ZERO_LESSONS_LEFT = "У вас осталось 0 занятий! Не забудьте пополнить баланс!"; // когда у ученика осталось 0 занятия
        public const string NOTIF_LESSON_WAS_RESCHEDULED_FOR_STUDENT = "Внимание! Занятие с репетитором {name} перенесено на {date}"; // перенос
        #endregion

        #region Уведомления менеджера
        public const string NOTIF_REGULAR_RESCHEDULE = "Постоянный перенос занятия: {tutorName} с {studentName} с {oldDate} на {newDate}!"; // постоянный перенос
        public const string NOTIF_NEW_STUDENT_FOR_MANAGER = "Новый ученик: {studentName} теперь занимается с {tutorName}"; // новый ученик записался
        public const string NOTIF_ZERO_LESSONS_LEFT_FOR_MANAGER = "0 занятий: напомните ученику {name} пополнить баланс!"; // 0 занятий у ученика
        public const string NOTIF_NEW_LESSON = "Добавление занятия: {studentName} занимается с {tutorName} {date}"; // добавлено занятие
        public const string NOTIF_REMOVE_LESSON = "Удаление занятия: {tutorName} удалил занятие с {studentName} в {date}"; // удалено занятие
        #endregion
    }

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
        public static List<User> UserList = new List<User>
            {
             new User() {FirstName = "Сергей", LastName = "Глузер", About = "Лучший", BirthDate = DateTime.Parse("15.02.2001"),
                    Courses = TestData.Courses.Where(m => m.Title == "Общий английский").ToList(), UserId =0, Email = "b", Phone = "+79054769537",
                    PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg", Password = "123", Role="Tutor", UserDates= new UserDate(){
                        dateTimes = new List<DateTime>(){ }
                    }},

                new User() {FirstName = "Иван", LastName = "Петров", About = "Почти лучший", BirthDate = DateTime.Parse("14.01.2002"),
                    Courses = Courses.Where(m => m.Title == "ОГЭ").ToList(), UserId=1, Email = "a.@mail.ru", Phone = "+79188703839",
                    PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg", Password = "123", Role="Tutor", UserDates= new UserDate(){
                        dateTimes = new List<DateTime>(){ }
                    }},
                new User
                {
                    FirstName = "Петр",
                    LastName = "Иванов",
                    Password = "448",
                    Role = "Student",
                     PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg",
                    Email = "a@mail.ru",
                    Phone = "+79188793839",
                    UserId = 3
                },
                new User
                {
                    FirstName = "Сергей",
                    LastName = "Курочка",
                    LessonsCount = 2,
                    Password = "123123",
                    Role = "Student",
                    Email = "a",
                     PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg",
                    Phone = "+79188793839",
                    UserId = 4
                },
                  new User
                {
                    FirstName = "Андрей",
                    LastName = "Глазев",
                    LessonsCount = 0,
                    Password = "123",
                     PhotoUrl = "https://i04.fotocdn.net/s119/486552b264ee5e3f/gallery_m/2711016530.jpg",
                    Role = "Manager",
                    Email = "god",
                    Phone = "+79054769537",
                    UserId = 5
                }
            };
        public static List<Notifications> Notifications = new List<Notifications>();

        public static List<User> Tutors = UserList.Where(m => m.Role == "Tutor").ToList();
        public static List<User> Managers = UserList.Where(m => m.Role == "Manager").ToList();

        public static List<Schedule> Schedules = new List<Schedule>() {
            new Schedule() { Id = 0, UserName = "Петр Иванов", TutorFullName = "Иван Петров", TutorId = 1, UserId = 3,

                StartDate = DateTime.Parse("09.02.2023 23:00"), Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("09.02.2023 23:00") } }, Course = Courses[0] },


            new Schedule() { Id = 1, UserName ="Петр Иванов", TutorFullName = "Сергей Глузер", TutorId = 0, UserId = 3,
             StartDate =    DateTime.Parse("10.02.2023 20:00"), Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("10.02.2023 20:00") } }, Course = Courses[1]},


            new Schedule() { Id = 3, UserName ="Сергей Курочка", TutorFullName = "Сергей Глузер", TutorId = 0, UserId = 4,
            StartDate = DateTime.Parse("26.02.2023 13:00"),
                Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("26.02.2023 13:00") } }, Looped = true, Course = Courses[2]},

            new Schedule() { Id = 5, UserName ="Сергей Курочка",TutorFullName = "Иван Петров", TutorId = 1, UserId = 4,
            StartDate = DateTime.Parse("05.03.2023 20:18"),
                Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("05.03.2023 20:18") } }, Looped = false, Course = Courses[0]},

            new Schedule() { Id = 3, UserName ="Сергей Курочка",TutorFullName = "Иван Петров", TutorId = 1, UserId = 4,
                StartDate=DateTime.Parse("17.03.2023 7:00"), Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("17.03.2023 7:00") } }, Looped = true, Course = Courses[1]},

            new Schedule() { Id = 3, UserName ="Сергей Курочка",TutorFullName = "Сергей Глузер", TutorId = 0, UserId = 4,
                 StartDate = DateTime.Parse("17.03.2023 7:00"),
                Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("17.03.2023 7:00") } }, Looped = true, Course = Courses[1]},

            new Schedule() { Id = 4, UserName ="Сергей Курочка", TutorFullName = "Сергей Глузер", TutorId = 0, UserId = 4,
                StartDate = DateTime.Parse("13.03.2023 17:00"),
                Date = new UserDate() { dateTimes = new List<DateTime>() { DateTime.Parse("13.03.2023 17:00") } }, Looped = true, Course = Courses[2]}};

        public static List<ChatUser> ChatUsers { get; set; } = new List<ChatUser>();
    }

    public class RescheduledLessons
    {
        public int UserId { get; set; }
        public int TutorId { get; set; }
        public DateTime OldTime { get; set; }
        public DateTime NewTime { get; set; }
        public string Reason { get; set; }
        public string Initiator { get; set; }
    }

    public class Notifications
    {
        public int Id { get; set; }
        public bool Readed { get; set; } = false;
        public int UserIdTo { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
    }
    public class NotificationTokens
    {
        public Dictionary<string, string> Tokens { get; set; } = new Dictionary<string, string>();
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
        public int UserId { get; set; }
    }

    public class Messages
    {
        public DateTime MessageTime { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }
    }

    public class InChat
    {
        public string UserId { get; set; }
        public string WithUserId { get; set; }
    }
}

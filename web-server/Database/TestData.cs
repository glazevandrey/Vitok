using System;
using System.Collections.Generic;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;


namespace web_server.DbContext
{
    public static class TestData
    {
        public static List<GoalDTO> Goals { get; set; } = new List<GoalDTO>() { 
            new GoalDTO() { Title = "Сдать экзамен" },
            new GoalDTO() {Title = "Изучить с нуля" },
            new GoalDTO() {  Title = "Работать" }, 
            new GoalDTO() {  Title = "Преодолеть языковой барьер" },
            new GoalDTO() {Title = "Проходить собеседование" },
            new GoalDTO() {  Title = "Путешествовать" }, 
            new GoalDTO() { Title = "Повысить уровень" },
            new GoalDTO(){Title = "Сдать международный экзамен"},
            };


        public static List<CourseDTO> Courses { get; set; } = new List<CourseDTO>() {
            new CourseDTO() {Title="ОГЭ", GoalId  = 1},
new CourseDTO() {Title="ЕГЭ",  GoalId = 1 },
new CourseDTO() {Title="С нуля", GoalId = 2},
new CourseDTO() {Title="Travel", GoalId = 6},
new CourseDTO() {Title="Общий английский", GoalId = 7},
new CourseDTO() {Title="Разговорный английский", GoalId = 4},
new CourseDTO() {Title="Бизнес английский", GoalId = 5},
new CourseDTO() {Title="Английский в профсреде", GoalId = 3},
new CourseDTO() {Title="IELTS", GoalId = 8},
new CourseDTO() {Title="TOEFL", GoalId = 8},
new CourseDTO() {Title="Cambridge ESOL", GoalId = 8}

            //new CourseDTO() {Title="ОГЭ", GoalId  = 4 ,},
            //new CourseDTO() { Title="ЕГЭ",  GoalId = 2, },
            //new CourseDTO() {Title="Общий английский", GoalId = 2}
        };
        public static List<SiteContact> Sites = new List<SiteContact>()
        {
            new SiteContact()
            {
                Text = "8-800-800-800",
                Title="Горячая линия"
            },
            new SiteContact()
            {
                Text = "Telegram: @vitok",
                Title="Мы в соцсетях"
            },
        };
        public static List<Tariff> Tariffs = new List<Tariff>()
        {
            new Tariff() {  Amount = 2700,  LessonsCount = 3, Title = "ПАКЕТ занятий 3"},
            new Tariff() { Amount = 7650, LessonsCount = 9, Title = "ПАКЕТ занятий 9"},
            new Tariff() { Amount = 21600, LessonsCount = 27, Title = "ПАКЕТ занятий 27"},
            new Tariff() { Amount = 56700, LessonsCount = 81, Title = "ВЫГОДНЫЙ ПАКЕТ ЗАНЯТИЙ 81"}
        };

        public static List<SiteContact> Contacts = new List<SiteContact> { new SiteContact() { Title = "Телефон", Text = "+790523232" }, new SiteContact { Title = "Телеграм", Text = "@andreyglazev" } };
        public static List<Registration> Registations = new List<Registration>();
        public static List<RescheduledLessons> RescheduledLessons = new List<RescheduledLessons>();

        public static List<Notifications> Notifications = new List<Notifications>();
        public static List<ScheduleDTO> Schedules = new List<ScheduleDTO>() {
           


        };

        public static List<UserDTO> UserList = new List<UserDTO>()
        {
            new StudentDTO()
            {
                FirstName = "Сергей",
LastName = "Воронов",
MiddleName = "Андреевич",
LessonsCount = 1,
Money = new List<UserMoney>(){ new UserMoney() { Cost = 1000, Count = 1} },
BirthDate = DateTime.Parse("12.05.1999"),
Password = "1234567",
WasFirstPayment = true,
Role = "Student",
Email = "sergey_voronov@mail.ru",
PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg",
Phone = "+79188793839",

            },
            new StudentDTO()
            {
                FirstName = "Илья",
MiddleName = "Владимирович",
LastName = "Соколов",
Password = "1234567",
Role = "Student",
WasFirstPayment = true,
StartWaitPayment = DateTime.Now,
PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg",
Email = "ilya_sokolov@mail.ru",
BirthDate = DateTime.Parse("14.04.1998"),
Phone = "+79105679835",

            },
            new StudentDTO()
            {
                FirstName = "Мария",
MiddleName = "Васильевна",
LastName = "Елисеева",
Password = "1234567",
Role = "Student",
WasFirstPayment = true,
StartWaitPayment = DateTime.Now,
PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg",
Email = "mary_eliseeva@mail.ru",
BirthDate = DateTime.Parse("01.10.1991"),
Phone = "+79063150326",

            },
            new StudentDTO()
            {
                FirstName = "Дмитрий",
MiddleName = "Анальтоевич",
LastName = "Муравьев",
Password = "1234567",
Role = "Student",
WasFirstPayment = true,
StartWaitPayment = DateTime.Now,
PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg",
Email = "dmitry_muraviev@mail.ru",
BirthDate = DateTime.Parse("18.07.2002"),
Phone = "+79105679835",

            },
            new StudentDTO()
            {
                FirstName = "Екатерина",
MiddleName = "Петровна",
LastName = "Ларионова",
Password = "1234567",
Role = "Student",
WasFirstPayment = true,
StartWaitPayment = DateTime.Now,
PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg",
Email = "kate_larionova@mail.ru",
BirthDate = DateTime.Parse("13.05.2005"),
Phone = "+79636879503",

            },
            new StudentDTO()
            {
                FirstName = "Алла",
MiddleName = "Максимовна",
LastName = "Цветкова",
Password = "1234567",
Role = "Student",
WasFirstPayment = true,
StartWaitPayment = DateTime.Now,
PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg",
Email = "alla_tsvetkova@mail.ru",
BirthDate = DateTime.Parse("13.05.2007"),
Phone = "+79618734467",

            },
            new ManagerDTO()
            {
                FirstName = "Анна",
MiddleName = "Павловна",
LastName = "Макарова",
BirthDate = DateTime.Parse("02.03.1994"),
Password = "1234567",
PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg",
Role = "Manager",
Email = "makarova_vm@mail.ru",
Phone = "+79054769537",

            },

            new TutorDTO()
                {FirstName = "Иван", MiddleName="Сергеевич", LastName = "Трифонов", About = " Преподаю английский язык 10 лет. Выпустил более 200 учеников с баллом ЕГЭ > 85 и 150 учеников с баллом ОГЭ > 90. Помогу вам с легкостью изучить английский !", BirthDate = DateTime.Parse("14.01.2002"), UserDates = new List<UserDate>(){ },
                   // Courses = TestData.Courses.Where(m => m.Title == "ОГЭ").ToList(),  },

                Email = "ivan_trifonov@mail.ru", Phone = "+79089385028",
                    PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg", Password = "1234567", Role="Tutor" },
            new TutorDTO()     {FirstName = "Марк", MiddleName="Алексеевич", LastName = "Тихонов", About = " Окончил ИВГУ с отличием. Преподаю английский язык 5 лет. ", BirthDate = DateTime.Parse("14.01.2002"), UserDates = new List<UserDate>(){ },
Email = "mark_tihonov@mail.ru", Phone = "+79057039923",
PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg", Password = "1234567", Role="Tutor" },
            new TutorDTO()
{FirstName = "Карина", MiddleName="Романовна", LastName = "Уварова", About = " Преподаю английский язык 5 лет. Выпустила более 100 учеников с баллом ЕГЭ > 85 и 75 учеников с баллом ОГЭ > 90", BirthDate = DateTime.Parse("05.12.1997"), UserDates = new List<UserDate>(){ }, Email = "karina_uvarova@mail.ru", Phone = "+79547803756", PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg", Password = "1234567", Role="Tutor" },
            new TutorDTO(){FirstName = "Александр", MiddleName="Дмитриевич", LastName = "Михайлов", About = " Преподаю английский язык 5 лет. Выпустил более 100 учеников с баллом ЕГЭ > 85 и 75 учеников с баллом ОГЭ > 90", BirthDate = DateTime.Parse("08.11.1999"), UserDates = new List<UserDate>(){ }, Email = "alexander_mihailov@mail.ru", Phone = "+79527803756", PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg", Password = "1234567", Role="Tutor" },
            new TutorDTO(){FirstName = "Елизавета", MiddleName="Олеговна", LastName = "Клюка", About = " Преподаю английский язык 6 лет. Выпустила более 100 учеников с баллом ЕГЭ > 85 и 75 учеников с баллом ОГЭ > 90", BirthDate = DateTime.Parse("10.09.2001"), UserDates = new List<UserDate>(){ }, Email = "elizabeth_kluka@mail.ru", Phone = "+79023801178", PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg", Password = "1234567", Role="Tutor" },
          new TutorDTO(){FirstName = "Егор", MiddleName="Данилович", LastName = "Абрамов", About = " Преподаю английский язык 6 лет. Выпустил более 100 учеников с баллом ЕГЭ > 85 и 75 учеников с баллом ОГЭ > 90", BirthDate = DateTime.Parse("10.02.1999"), UserDates = new List<UserDate>(){ }, Email = "egor_abramov@mail.ru", Phone = "+79116790554", PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg", Password = "1234567", Role="Tutor" },
            new TutorDTO()
 {FirstName = "Маргарита", MiddleName="Павловна", LastName = "Волкова", About = " Преподаю английский язык 20 лет. Выпустила более 200 учеников с баллом ЕГЭ > 90 и 150 учеников с баллом ОГЭ > 90", BirthDate = DateTime.Parse("10.11.1980"), UserDates = new List<UserDate>(){ }, Email = "margo_volkova@mail.ru", Phone = "+79158476586", PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg", Password = "1234567", Role="Tutor" },
            new TutorDTO(){FirstName = "Светлана", MiddleName="Николаевна", LastName = "Примакова", About = " Преподаю английский язык 20 лет. Готовлю учеников к сдаче международных экзаменов на высокий балл!", BirthDate = DateTime.Parse("10.11.1980"), UserDates = new List<UserDate>(){ }, Email = "svetlana_primakova@mail.ru", Phone = "+79108476758", PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg", Password = "1234567", Role="Tutor" },
         new TutorDTO(){FirstName = "Ольга", MiddleName="Алексеевна", LastName = "Жукова", About = " Преподаю английский язык 8 лет.", BirthDate = DateTime.Parse("10.11.1998"), UserDates = new List<UserDate>(){ }, Email = "olga_zhukova@mail.ru", Phone = "+79065456789", PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg", Password = "1234567", Role="Tutor" },
        new TutorDTO(){FirstName = "Роман", MiddleName="Алексеевич", LastName = "Карпов", About = " Преподаю английский язык 10 лет.", BirthDate = DateTime.Parse("10.11.1993"), UserDates = new List<UserDate>(){ }, Email = "roman_karpov@mail.ru", Phone = "+79065456789", PhotoUrl = "https://mirvu.ru/wp-content/uploads/2022/03/sbkgl.jpg", Password = "1234567", Role="Tutor" },
        };
    }
}

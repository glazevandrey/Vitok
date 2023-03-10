using System;
using System.Collections.Generic;
using web_server.DbContext;

namespace web_server.Models
{
    public class User : TransferModel
    {

        // общие поля
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string PhotoUrl { get; set; } = "https://static3.vivoo.ru/datas/photos/800x800/d6/96/04f0912d55d8dc01ed36d15927dd.jpg?1";
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public NotificationTokens NotificationTokens { get; set; } = new NotificationTokens();
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string ActiveToken { get; set; }
        public BalanceHistory BalanceHistory { get; set; } = new BalanceHistory();
        public double Balance { get; set; }

        // поля студента
        public string Wish { get; set; }
        public int LessonsCount { get; set; }
        public bool UsedTrial { get; set; } = false;

        // поля Репетитора
        public List<Course> Courses { get; set; }
        public string About { get; set; }
        public UserDate UserDates { get; set; }
    }
}

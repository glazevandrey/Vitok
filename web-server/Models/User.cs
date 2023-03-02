using System;
using System.Collections.Generic;

namespace web_server.Models
{
    public class User
    {

        // общие поля
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string PhotoUrl { get; set; }
        public string LastName { get; set; }
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

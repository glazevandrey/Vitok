using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using web_server.DbContext;

namespace web_server.Models
{
    public class User : TransferModel
    {

        // общие поля
        [Key]
        public int UserId { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string PhotoUrl { get; set; } = "/default_img.jpg";
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public List<NotificationTokens> NotificationTokens { get; set; } = new List<NotificationTokens>();
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string ActiveToken { get; set; }
        public List<BalanceHistory> BalanceHistory { get; set; } = new List<BalanceHistory>();
        public double Balance { get; set; }

        // поля студента
        public string Wish { get; set; }
        public DateTime StartWaitPayment { get; set; }
        public bool WasFirstPayment { get; set; } = false;
        public bool FirstLogin { get; set; } = true;
        public int LessonsCount { get; set; }
        public int SkippedInThisMonth { get; set; } = 0;
        public List<UserMoney> Money { get; set; } = new List<UserMoney>();
        public List<UserCredit> Credit { get; set; } = new List<UserCredit>();

        public bool UsedTrial { get; set; } = false;

        // поля Репетитора
        public List<Course> Courses { get; set; }
        public string About { get; set; }
        public UserDate UserDates { get; set; }
    }   
}

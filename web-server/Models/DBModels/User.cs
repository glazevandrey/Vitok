using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_server.Models.DBModels
{
    public abstract class User : TransferModel
    {
        [Key]

        public int UserId { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string PhotoUrl { get; set; } = "/default_img.jpg";
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public List<NotificationTokens> NotificationTokens { get; set; } = new List<NotificationTokens>();
        public List<Notifications> Notifications { get; set; } = new List<Notifications>();
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string ActiveToken { get; set; }
        public List<BalanceHistory> BalanceHistory { get; set; } = new List<BalanceHistory>();
        public double Balance { get; set; }
        public List<Schedule> Schedules { get; set; } = new List<Schedule>();

        public Chat Chat { get; set; } = new Chat();

        // поля студента
        [NotMapped]

        public virtual string Wish { get; set; }
        [NotMapped]

        public virtual DateTime StartWaitPayment { get; set; }
        [NotMapped]

        public virtual bool WasFirstPayment { get; set; } = false;
        [NotMapped]

        public virtual bool FirstLogin { get; set; } = true;
        [NotMapped]

        public virtual int LessonsCount { get; set; }
        [NotMapped]

        public virtual int SkippedInThisMonth { get; set; } = 0;
        [NotMapped]

        public virtual List<UserMoney> Money { get; set; } = new List<UserMoney>();
        [NotMapped]

        public virtual List<UserCredit> Credit { get; set; } = new List<UserCredit>();
        [NotMapped]

        public virtual bool UsedTrial { get; set; } = false;

        // поля Репетитора
        [NotMapped]

        public virtual List<Course> Courses { get; set; }
        [NotMapped]

        public virtual string About { get; set; }
        [NotMapped]

        public virtual List<UserDate> UserDates { get; set; } = new List<UserDate>();


    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_server.Models.DBModels
{
    [Table("Students")]
    public class Student : User
    {

        // поля студента
        public new string Wish { get; set; }
        public new DateTime StartWaitPayment { get; set; }
        public new bool WasFirstPayment { get; set; } = false;
        public new bool FirstLogin { get; set; } = true;
        public new int LessonsCount { get; set; }
        public new int SkippedInThisMonth { get; set; } = 0;
        public new List<UserMoney> Money { get; set; } = new List<UserMoney>();
        public new List<UserCredit> Credit { get; set; } = new List<UserCredit>();
        public new bool UsedTrial { get; set; } = false;
    }
}

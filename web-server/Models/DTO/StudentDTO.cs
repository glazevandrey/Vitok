using System.Collections.Generic;
using System;
using web_server.Models.DBModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_server.Models.DTO
{
    public class StudentDTO : UserDTO
    {
        // поля студента
        public string Wish { get; set; }
        public DateTime StartWaitPayment { get; set; }
        public bool WasFirstPayment { get; set; } = false;
        public bool FirstLogin { get; set; } = true;
        public int LessonsCount { get; set; }
        public int SkippedInThisMonth { get; set; } = 0;

        [NotMapped]
        public List<UserMoney> Money { get; set; } = new List<UserMoney>();
        
        [NotMapped]
        public List<UserCredit> Credit { get; set; } = new List<UserCredit>();
        public bool UsedTrial { get; set; } = false;
    }
}

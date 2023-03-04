using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using web_server.DbContext;

namespace web_server.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TutorId { get; set; }
        public string TutorFullName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }
        public Course Course { get; set; }
        public bool Looped { get; set; } = true;
        public UserDate Date { get; set; }
        public DateTime StartDate { get; set; }
        public Status Status { get; set; } = Status.Ожидает;
        public Dictionary<string, bool> Tasks = new Dictionary<string, bool>()
        {
            { Constatnts.NOTIF_START_LESSON, false},
            { Constatnts.NOTIF_TOMORROW_LESSON, false},
        };
        public int RescheduledId { get; set; } = -1;
        public List<RescheduledLessons> RescheduledLessons { get; set; } = new List<RescheduledLessons>();
        public DateTime RescheduledDate { get; set; } 

        public List<DateTime> ReadyDates { get; set; } = new List<DateTime>();
        public DateTime RemoveDate { get; set; }
    }
}

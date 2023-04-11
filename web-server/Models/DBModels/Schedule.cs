using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using web_server.Models.DBModels;

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

        public int CourseId { get; set; }

        [NotMapped,ForeignKey("CourseId")]
        public Course Course { get; set; }
        public bool Looped { get; set; } = true;

        public int DateId { get; set; }

        [NotMapped, ForeignKey("DateId")]
        public UserDate Date { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Status Status { get; set; } = Status.Ожидает;

        [NotMapped]

        public List<NotificationTask> Tasks = new List<NotificationTask>()
        {
            new NotificationTask() { NotifKey = Constants.NOTIF_START_LESSON, NotifValue = false },
            new NotificationTask() { NotifKey = Constants.NOTIF_TOMORROW_LESSON, NotifValue = false },
            new NotificationTask() { NotifKey = Constants.NOTIF_DONT_FORGET_SET_STATUS, NotifValue = false }
        };

        [NotMapped]
        public List<SkippedDate> SkippedDates { get; set; } = new List<SkippedDate>();

        [NotMapped]
        public List<RescheduledLessons> RescheduledLessons { get; set; } = new List<RescheduledLessons>();
        public DateTime RescheduledDate { get; set; }
        public DateTime NewDate { get; set; }
        public DateTime WaitPaymentDate { get; set; }
        [NotMapped]

        public List<PaidLesson> PaidLessons { get; set; } = new List<PaidLesson>();
        [NotMapped]

        public List<ReadyDate> ReadyDates { get; set; } = new List<ReadyDate>();
        public DateTime RemoveDate { get; set; }
    }
    public class ReadyDate 
    {
        [Key]
        public int Id { get; set;}
        public DateTime Date { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        public List<NotificationTask> Tasks = new List<NotificationTask>()
        {
            new NotificationTask() { NotifKey = Constants.NOTIF_START_LESSON, NotifValue = false },
            new NotificationTask() { NotifKey = Constants.NOTIF_TOMORROW_LESSON, NotifValue = false },
            new NotificationTask() { NotifKey = Constants.NOTIF_DONT_FORGET_SET_STATUS, NotifValue = false }
        };

        public List<DateTime> SkippedDates { get; set; } = new List<DateTime>();
        public List<RescheduledLessons> RescheduledLessons { get; set; } = new List<RescheduledLessons>();
        public DateTime RescheduledDate { get; set; }
        public DateTime NewDate { get; set; }
        public DateTime WaitPaymentDate { get; set; }

        public List<DateTime> ReadyDates { get; set; } = new List<DateTime>();
        public DateTime RemoveDate { get; set; }
    }
}

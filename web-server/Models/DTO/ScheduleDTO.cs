using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server.Models.DTO
{
    public class ScheduleDTO
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public StudentDTO Student { get; set; }
        public int TutorId { get; set; }
        public TutorDTO Tutor { get; set; }

        public string TutorFullName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }

        public int CourseId { get; set; }

        public CourseDTO Course { get; set; }
        public bool Looped { get; set; } = true;

        public int DateId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Status Status { get; set; } = Status.Ожидает;

        public List<NotificationTaskDTO> Tasks { get; set; } = new List<NotificationTaskDTO>(){
            new NotificationTaskDTO() { NotifKey = Constants.NOTIF_START_LESSON, NotifValue = false },
            new NotificationTaskDTO() { NotifKey = Constants.NOTIF_TOMORROW_LESSON, NotifValue = false },
            new NotificationTaskDTO() { NotifKey = Constants.NOTIF_DONT_FORGET_SET_STATUS, NotifValue = false }
        };

        public List<SkippedDate> SkippedDates { get; set; } = new List<SkippedDate>();

        public List<RescheduledLessons> RescheduledLessons { get; set; } = new List<RescheduledLessons>();
        public DateTime RescheduledDate { get; set; }
        public DateTime NewDate { get; set; }
        public DateTime WaitPaymentDate { get; set; }
        public List<PaidLesson> PaidLessons { get; set; } = new List<PaidLesson>();
        public List<ReadyDate> ReadyDates { get; set; } = new List<ReadyDate>();
        public DateTime RemoveDate { get; set; }
    }
    public class ReadyDate
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
    }
}

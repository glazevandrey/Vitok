using System;
using web_server.Models.DBModels;

namespace web_server.Models.V2
{
    public class LessonOccurrenceDto
    {
        public long ScheduleId { get; set; }

        public DateTime Start { get; set; }

        public long StudentId { get; set; }

        public long TutorId { get; set; }

        public string StudentName { get; set; }

        public string CourseTitle { get; set; }

        public Status Status { get; set; }

        public bool IsLooped { get; set; }

        public PaymentWarning PaymentWarning { get; set; }

        public RescheduledLessons RescheduleInfo { get; set; }
    }

    public class PaymentWarning
    {
        public bool IsActive { get; set; }
        public DateTime DeadlineUtc { get; set; }
        public double MinutesLeft { get; set; }
    }
}

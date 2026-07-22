using System;
using System.Collections.Generic;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server.Models.V2
{
    public class ScheduleWeekResponseDto
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public List<LessonOccurrenceDto> Lessons { get; set; } = new List<LessonOccurrenceDto>();
    }

    //public class Lesson
    //{
    //    public long Id { get; set; }
    //    public DateTime Start { get; set; }
    //    public DateTime End { get; set; }
    //    public long StudentId { get; set; }
    //    public long TutorId { get; set; }
    //    public string CourseTitle { get; set; }
    //    public Status Status { get; set; } = Status.Ожидает;
    //    public bool IsLooped { get; set; }
    //    public DateTime RescheduleDate { get; set; }
    //    public PaymentWarning PaymentWarning { get; set; }
    //    public List<RescheduleInfo> RescheduleDates { get; set; }
    //    public List<SkippedDate> SkippedDates { get; set; }
    //    public List<ReadyDate> ReadyDates { get; set; }
    //}

    //public class RescheduleInfo
    //{
    //    public DateTime OldStartUtc { get; set; }
    //    public DateTime NewStartUtc { get; set; }
    //    public string Reason { get; set; }
    //    public string Initiator { get; set; }
    //    public bool IsPermanent { get; set; }
    //}

    //public class PaymentWarning
    //{
    //    public bool IsActive { get; set; }
    //    public int MinutesLeft { get; set; }
    //    public DateTime DeadlineUtc { get; set; }
    //}
}
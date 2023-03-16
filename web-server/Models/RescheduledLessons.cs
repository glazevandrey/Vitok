using System;

namespace web_server.Models
{
    public class RescheduledLessons
    {
        public int UserId { get; set; }
        public int TutorId { get; set; }
        public DateTime OldTime { get; set; }
        public DateTime NewTime { get; set; }
        public string Reason { get; set; }
        public string Initiator { get; set; }
    }
}

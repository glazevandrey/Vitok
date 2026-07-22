using System;

namespace web_server.Models.V2
{
    public class RescheduleLessonDto
    {
        public int Status { get; set; }
        public long ScheduleId { get; set; } 
        public DateTime NewStart { get; set; }
        public DateTime OldDate { get; set; }
        public string Reason { get; set; }
        public string Initiator { get; set; }
        public bool IsLooped { get; set; }
    }
}

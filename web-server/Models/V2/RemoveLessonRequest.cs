using System;

namespace web_server.Models.V2
{
    public class RemoveLessonRequest
    {
        public int TutorId { get; set; }
        public int UserId { get; set; }
        public int ScheduleId { get; set; }        
        public DateTime Curr { get; set; }
    }
}

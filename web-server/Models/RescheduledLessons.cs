using System;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class RescheduledLessons
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TutorId { get; set; }
        public DateTime OldTime { get; set; }
        public DateTime NewTime { get; set; }
        public string Reason { get; set; }
        public string Initiator { get; set; }
    }
}

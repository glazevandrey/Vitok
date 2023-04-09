using System;

namespace web_server.Models.DBModels
{
    public class UserCredit
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public int TutorId { get; set; }
        public int ScheduleId { get; set; }
        public DateTime ScheduleSkippedDate { get; set; }
        public bool Repaid { get; set; }
    }
}

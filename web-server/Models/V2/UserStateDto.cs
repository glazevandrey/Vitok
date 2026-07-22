using System;

namespace web_server.Models.V2
{
    public class UserStateDto
    {
        public bool IsFirstLogin { get; set; }
        public bool IsFirstPayment { get; set; }
        public DateTime? WaitPaymentStart { get; set; }
        public int LessonsCount { get; set; }
    }
}

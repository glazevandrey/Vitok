using System;

namespace web_server.Models.V2
{
    public class PaymentStateDto
    {
        public bool HasDebt { get; set; }
        public DateTime? DebtUntil { get; set; }
        public int FreeHoursLeft { get; set; }
    }
}

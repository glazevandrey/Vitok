using System;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class BalanceHistory
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public CashFlow CashFlow { get; set; }

        public CustomMessage CustomMessages { get; set; }

    }
}
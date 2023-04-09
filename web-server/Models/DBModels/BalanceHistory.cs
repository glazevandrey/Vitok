using System;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DBModels
{
    public class BalanceHistory
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public CashFlow CashFlow { get; set; }
        public string CustomMessage { get; set; }

    }
}
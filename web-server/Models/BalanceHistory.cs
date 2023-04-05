using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class BalanceHistory
    {
        [Key]
        public int Id { get; set; }
        // Для репета
        public List<CashFlow> CashFlow { get; set; } = new List<CashFlow>();

        // Для студента
        public List<CustomMessage> CustomMessages { get; set; } = new List<CustomMessage>();

    }
}
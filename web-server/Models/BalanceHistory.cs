using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class BalanceHistory
    {
        [Key]
        public int Id { get; set; }

        public List<CashFlow> CashFlow { get; set; } = new List<CashFlow>();

        public List<CustomMessage> CustomMessages { get; set; } = new List<CustomMessage>();

    }
}
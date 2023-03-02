using System;
using System.Collections.Generic;

namespace web_server.Models
{
    public class BalanceHistory
    {

        // Для репета
        public List<CashFlow> CashFlow { get; set; } = new List<CashFlow>();

        // Для студента
        public Dictionary<DateTime, string> CustomMessages { get; set; } = new Dictionary<DateTime, string>();

    }
}
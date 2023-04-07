using System;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class CashFlow
    {
        [Key]
        public int Id { get; set; }
        public int Amount { get; set; }
    }
}
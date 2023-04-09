using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DBModels
{
    public class CashFlow
    {
        [Key]
        public int Id { get; set; }
        public int Amount { get; set; }
        public int Count { get; set; }
    }
}
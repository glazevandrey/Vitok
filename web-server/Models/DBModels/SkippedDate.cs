using System;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DBModels
{
    public class SkippedDate
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public bool WasWarn { get; set; }
        public int InitPaid { get; set; } = 0;
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class CustomMessage
    {
        [Key]
        public int Id { get; set; }
        public DateTime MessageKey { get; set; }
        public string MessageValue { get; set; }
    }
}

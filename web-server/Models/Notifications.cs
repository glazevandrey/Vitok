using System;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class Notifications
    {
        [Key]
        public int Id { get; set; }
        public bool Readed { get; set; } = false;
        public int UserIdTo { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
    }
}

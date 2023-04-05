using System;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class Messages
    {
        [Key]
        public int Id { get; set; }
        public DateTime MessageTime { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }
    }
}

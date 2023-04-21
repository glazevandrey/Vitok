using System;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DTO
{
    public class NotificationsDTO
    {
        [Key]
        public int Id { get; set; }
        public bool Readed { get; set; } = false;
        public int UserId { get; set; }
        public UserDTO User { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
    }
}

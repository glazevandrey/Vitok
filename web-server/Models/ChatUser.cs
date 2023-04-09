using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class ChatUser
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<ConnectionToken> ConnectionTokens { get; set; } = new List<ConnectionToken>();
        public List<Messages> Messages { get; set; } = new List<Messages>();
        public List<Contact> Contacts { get; set; } = new List<Contact>();
    }
}

using System.Collections.Generic;

namespace web_server.Models
{
    public class ChatUser
    {
        public int UserId { get; set; }
        public Dictionary<string, string> ConnectionTokens { get; set; }
        public List<Messages> Messages { get; set; } = new List<Messages>();
        public List<Contact> Contacts { get; set; } = new List<Contact>();
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server.Models.DTO
{
    public class ChatDTO
    {
        [Key]
        public int Id { get; set;}
        public int UserId { get; set; }
        public UserDTO User { get; set; }
        public int InChat { get; set; } 
        public List<ConnectionToken> ConnectionTokens { get; set; } = new List<ConnectionToken>();
        public List<Messages> Messages { get; set; } = new List<Messages>();
        public List<Contact> Contacts { get; set; } = new List<Contact>();
    }
}

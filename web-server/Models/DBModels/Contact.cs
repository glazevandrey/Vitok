using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DBModels
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
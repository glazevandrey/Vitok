using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class InChat
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string WithUserId { get; set; }
    }
}

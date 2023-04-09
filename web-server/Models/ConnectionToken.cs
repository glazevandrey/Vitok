using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class ConnectionToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public string Status { get; set; }
    }
}

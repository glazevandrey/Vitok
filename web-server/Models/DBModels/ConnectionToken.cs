using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DBModels
{
    public class ConnectionToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public string Status { get; set; }
    }
}

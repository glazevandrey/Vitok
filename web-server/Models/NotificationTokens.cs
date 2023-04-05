using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class NotificationTokens
    {
        [Key]
        public int Id { get; set; }
        public string TokenKey { get; set; }
        public string TokenValue { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class NotificationTask
    {
        [Key]
        public int Id { get; set; }
        public string NotifKey { get; set; }
        public bool NotifValue { get; set; }
    }
}

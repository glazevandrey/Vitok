using System.ComponentModel.DataAnnotations;
using web_server.Models.DTO;

namespace web_server.Models.DBModels
{
    public class NotificationTask
    {
        [Key]
        public int Id { get; set; }
        public string NotifKey { get; set; }
        public bool NotifValue { get; set; }
        public int ScheduleId { get; set; }
        public ScheduleDTO Schedule { get; set; }
    }
}

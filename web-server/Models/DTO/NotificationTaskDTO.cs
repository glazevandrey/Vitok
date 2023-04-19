using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DTO
{
    public class NotificationTaskDTO
    {
        [Key]
        public int Id { get; set; }
        public string NotifKey { get; set; }
        public bool NotifValue { get; set; }
        public int ScheduleId { get; set; }
        public ScheduleDTO Schedule { get; set; }
    }
}

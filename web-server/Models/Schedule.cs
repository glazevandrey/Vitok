using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TutorId { get; set; }
        public string TutorFullName { get; set; }
        public string UserName { get; set; }
        public bool Looped { get; set; } = true;
        public UserDate Date { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class Registration : TransferModel
    {
        [Key]
        public int UserId { get; set; }
        public int TutorId { get; set; }
        public UserDate WantThis { get; set; }
        public Course Course { get; set; }
    }
}

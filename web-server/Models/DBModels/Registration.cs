using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DBModels
{
    public class Registration : TransferModel
    {
        [Key]
        public int UserId { get; set; }
        public int TutorId { get; set; }
        public List<UserDate> WantThis { get; set; }
        public Course Course { get; set; }
    }
}

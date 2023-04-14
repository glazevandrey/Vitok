using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using web_server.Models.DBModels;

namespace web_server.Models.DTO
{
    public class RegistrationDTO
    {
        [Key]
        public int UserId { get; set; }
        public int TutorId { get; set; }
        public List<UserDate> WantThis { get; set; }
        public CourseDTO Course { get; set; }
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using web_server.Models.DBModels;

namespace web_server.Models.DTO
{
    public class TutorDTO : UserDTO
    {
        [NotMapped]
        public List<Course> Courses { get; set; }
        public string About { get; set; }
        [NotMapped]
        public UserDate UserDates { get; set; }
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using web_server.Models.DTO;

namespace web_server.Models.DTO
{
    public class GoalDTO
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }

        public List<CourseDTO> Courses { get; set; }
    }
}

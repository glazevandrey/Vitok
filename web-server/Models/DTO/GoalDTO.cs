using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

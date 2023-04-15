using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DTO
{
    public class TutorCourse
    {

        [Key]
        public int Id { get; set; }
        public int? CourseId { get; set; }
        public CourseDTO Course { get; set; }
        public int? TutorId { get; set; }
        public TutorDTO  Tutor {get;set;}
    }
}

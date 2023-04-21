using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using web_server.Models.DBModels;

namespace web_server.Models.DTO
{
    public class TutorDTO : UserDTO
    {

        public string About { get; set; }

        [NotMapped]
        public List<CourseDTO> ConstCourses { get; set; } = new List<CourseDTO>();
        public List<TutorCourse> Courses { get; set; } = new List<TutorCourse>();

        public List<UserDate> UserDates { get; set; } = new List<UserDate>();
        public List<ScheduleDTO> Schedules { get; set; } = new List<ScheduleDTO>();
    }
}

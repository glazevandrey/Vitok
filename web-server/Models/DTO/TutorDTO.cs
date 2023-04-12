using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using web_server.Models.DBModels;
using web_server.Models.DBModels.DTO;

namespace web_server.Models.DTO
{
    public class TutorDTO : UserDTO
    {

        public List<CourseDTO> Courses { get; set; } = new List<CourseDTO>();
        public string About { get; set; }
        public List<UserDate> UserDates { get; set; }  = new List<UserDate>();

        public List<ScheduleDTO> Schedules { get; set; } = new List<ScheduleDTO>();
    }
}

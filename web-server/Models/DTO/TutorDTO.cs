using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using web_server.Models.DBModels;

namespace web_server.Models.DTO
{
    public class TutorDTO : UserDTO
    {

        public List<Course> Courses { get; set; } = new List<Course>();
        public string About { get; set; }
        public List<UserDate> UserDates { get; set; }  = new List<UserDate>();

        public List<ScheduleDTO> Schedules { get; set; } = new List<ScheduleDTO>();
    }
}

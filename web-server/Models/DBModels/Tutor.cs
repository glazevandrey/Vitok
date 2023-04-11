using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_server.Models.DBModels
{
    [Table("Tutors")]

    public class Tutor : User
    {
        // поля Репетитора
        public new  List<Course> Courses { get; set; }
        public new string About { get; set; }
        public new List<UserDate> UserDates { get; set; }
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DBModels
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public List<Goal> Goals { get; set; }
    }
}

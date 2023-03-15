using System.Collections.Generic;

namespace web_server.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<Goal> Goals { get; set; }
    }
}

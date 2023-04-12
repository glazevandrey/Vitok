using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_server.Models.DBModels
{
    public class Goal
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }

        public int? CourseId { get; set; }
        public Course Course { get; set; }
    }
}

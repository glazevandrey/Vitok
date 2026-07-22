using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DBModels
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }

        public Goal Goal { get; set; } // Навигационное свойство
    }
}

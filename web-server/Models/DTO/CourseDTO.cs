using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DTO
{
    public class CourseDTO
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }

        public int? GoalId { get; set; } // Внешний ключ
        public GoalDTO Goal { get; set; } // Навигационное свойство

    }
}

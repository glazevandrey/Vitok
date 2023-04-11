using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_server.Models.DBModels
{
    public class Course
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }

        public int GoalId { get; set; } // Внешний ключ

        [ForeignKey("GoalId")]
        public Goal Goal { get; set; } // Навигационное свойство


    }
}

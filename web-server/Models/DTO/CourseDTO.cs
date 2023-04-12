using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using web_server.Models.DTO;

namespace web_server.Models.DBModels.DTO
{
    public class CourseDTO
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }

        public int? GoalId { get; set; } // Внешний ключ
        public Goal Goal { get; set; } // Навигационное свойство


        public int? TutorId { get; set; }
        public TutorDTO Tutor {get;set;}


        public int? ScheduleId{ get; set; }
        public ScheduleDTO Schedule{ get; set; }


    }
}

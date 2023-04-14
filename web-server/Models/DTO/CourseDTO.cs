using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server.Models.DTO
{
    public class CourseDTO
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }

        public int? GoalId { get; set; } // Внешний ключ
        public GoalDTO Goal { get; set; } // Навигационное свойство

        //public int? TutorId { get; set; }
        //public List<TutorDTO> Tutor {get;set;}


        //public int? ScheduleId{ get; set; }
        //public List<ScheduleDTO> Schedule{ get; set; }


    }
}

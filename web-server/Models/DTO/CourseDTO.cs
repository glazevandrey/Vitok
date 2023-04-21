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

        //public int? TutorId { get; set; }
        //public List<TutorDTO> Tutor {get;set;}


        //public int? ScheduleId{ get; set; }
        //public List<ScheduleDTO> Schedule{ get; set; }


    }
}

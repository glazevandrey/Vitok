using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using web_server.Models.DTO;

namespace web_server.Models.DBModels
{
    public class RescheduledLessons
    {
        [Key]
        public int Id { get; set; }

        //[ForeignKey("Student")]
        //public int UserId { get; set; }

        //[ForeignKey("Tutor")]
        //public int TutorId { get; set; }
        public DateTime OldTime { get; set; }
        public DateTime NewTime { get; set; }
        public string Reason { get; set; }
        public string Initiator { get; set; }
        //public TutorDTO Tutor { get; set; } // Навигационное свойство
        //public StudentDTO Student{ get; set; } // Навигационное свойство


    }
}

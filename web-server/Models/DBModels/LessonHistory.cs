using System;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class LessonHistory
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Status Status { get; set; }

    }
}
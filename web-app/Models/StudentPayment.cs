using System;

namespace web_app.Models
{
    public class StudentPayment
    {
        public string StudentName { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime LessonDate { get; set; }
        public decimal LessonAmount { get; set; }
        public bool LessonLooped { get; set; }
        public int DebtLessons { get; set; }
    }
}

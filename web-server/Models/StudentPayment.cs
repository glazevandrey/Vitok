using System;

namespace web_app.Models
{
    public class StudentPayment
    {
        public string StudentName { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentAmount { get; set; }
        public DateTime LessonDate { get; set; }
        public int LessonAmount { get; set; }
        public bool LessonLooped { get; set; }
        public int DebtLessons { get; set; }
    }
}

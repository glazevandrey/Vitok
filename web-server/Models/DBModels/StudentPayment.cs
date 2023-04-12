﻿using System;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DBModels
{
    public class StudentPayment
    {
        [Key]
        public int Id { get; set; }
        public string StudentName { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentAmount { get; set; }
        public DateTime LessonDate { get; set; }
        public int LessonAmount { get; set; }
        public bool LessonLooped { get; set; }
        public int DebtLessons { get; set; }
    }
}
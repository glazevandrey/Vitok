﻿using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DBModels
{
    public class Tariff
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public int LessonsCount { get; set; }
        public double Amount { get; set; }
    }
}
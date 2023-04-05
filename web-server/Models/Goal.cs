﻿using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class Goal
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
    }
}

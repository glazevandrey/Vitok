﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using web_server.Models.DTO;

namespace web_server.Models.DBModels
{
    public class Goal
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
    }
}

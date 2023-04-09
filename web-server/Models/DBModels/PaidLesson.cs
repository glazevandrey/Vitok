using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DBModels
{
    public class PaidLesson
    {
        [Key]
        public int Id { get; set; }
        public DateTime PaidDate { get; set; }
        public int PaidCount { get; set; }
    }
}

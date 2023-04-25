﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using web_server.Models.DBModels;

namespace web_server.Models.DTO
{
    public class RegistrationDTO
    {
        [Key]
        public int Id { get; set; }
        public int ExistUserId { get; set; }
        public Guid NewUserGuid { get; set; }
        public int TutorId { get; set; }
        public List<UserDate> WantThis { get; set; }
        public CourseDTO Course { get; set; }
        public int CourseId { get; set; }
    }
}

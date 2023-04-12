using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using web_server.Models.DTO;

namespace web_server.Models.DBModels
{
    public class UserDate : TransferModel
    {
        [Key]
        public int Id { get; set; }
        public DateTime dateTime { get; set; }

        public int TutorId { get; set; }
        public TutorDTO Tutor { get; set; }
    }
}

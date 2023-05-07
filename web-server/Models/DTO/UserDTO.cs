using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using web_server.Models.DBModels;

namespace web_server.Models.DTO
{
    public class UserDTO
    {
        [Key]
        public int UserId { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string PhotoUrl { get; set; } = "/content/images/default-avatar.svg";
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public List<NotificationTokens> NotificationTokens { get; set; } = new List<NotificationTokens>();
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string ActiveToken { get; set; }
        public ChatDTO Chat { get; set; }
        public List<NotificationsDTO> Notifications { get; set; } = new List<NotificationsDTO>();
        public List<BalanceHistory> BalanceHistory { get; set; } = new List<BalanceHistory>();
        public double Balance { get; set; }
    }
}

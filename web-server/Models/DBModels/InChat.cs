using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using web_server.Models.DBModels;

namespace web_server.Models
{
    public class InChat
    {
        [Key]
        public int Id { get; set; }
        public int WithUserId { get; set; }
    }
}

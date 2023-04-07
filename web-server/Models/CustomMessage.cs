using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class CustomMessage
    {
        [Key]
        public int Id { get; set; }
        public string MessageValue { get; set; }
    }
}

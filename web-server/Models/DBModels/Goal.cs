using System.ComponentModel.DataAnnotations;

namespace web_server.Models.DBModels
{
    public class Goal
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
    }
}

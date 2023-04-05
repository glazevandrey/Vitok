using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class SiteContact
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
    }
}

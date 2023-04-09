using System.ComponentModel.DataAnnotations;

namespace web_server.Models
{
    public class FileAttachment
    {
        [Key]
        public int Id { get; set; }
        public string fileName { get; set; }
        public string content { get; set; }
    }
    public class FileAttachmentCsv
    {
        [Key]
        public int Id { get; set; }
        public string fileName { get; set; }
        public string fileData { get; set; }
    }
}

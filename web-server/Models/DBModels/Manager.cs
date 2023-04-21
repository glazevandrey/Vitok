using System.ComponentModel.DataAnnotations.Schema;

namespace web_server.Models.DBModels
{
    [Table("Managers")]

    public class Manager : User
    {
        public bool isAdmin { get; set; } = true;
    }
}

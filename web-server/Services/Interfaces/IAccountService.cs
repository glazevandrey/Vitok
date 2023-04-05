using Microsoft.AspNetCore.Http;
using web_server.Models;

namespace web_server.Services.Interfaces
{
    public interface IAccountService
    {
        public User SaveAccountInfo(string args);
        public string SavePhoto(IFormFile file, string id);
        public bool Withdraw(string tutorid, string count);

    }
}

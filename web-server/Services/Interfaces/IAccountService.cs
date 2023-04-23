using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using web_server.Models.DBModels;

namespace web_server.Services.Interfaces
{
    public interface IAccountService
    {
        public Task<User> SaveAccountInfo(string args);
        public Task<bool> RemoveFirstLogin(string args);
        public Task<string> SavePhoto(IFormFile file, string id);
        public Task<bool> Withdraw(string tutorid, string count);
        public Task<List<User>> GetAllUserContacts(string id, string role);

    }
}

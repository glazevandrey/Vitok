using Microsoft.AspNetCore.Http;
using web_server.Models;

namespace web_server.Services
{
    public interface IAuthService
    {
        public string LogIn(User user, HttpContext context);
        public string Register(User user, HttpContext context);
        public string GetUserByToken(string token);
        public string GetUserById(string id);
        public bool CheckIsActive(HttpContext context);

    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using web_server.Models.DBModels;

namespace web_server.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<string> LogIn(string login, string password, string guid, HttpContext context);
        public Task<string> LogIn(string login, string password, HttpContext context);
        public Task<string> Register(User user, string guid, HttpContext context, IHubContext<NotifHub> _hubContext);
        public Task<string> GetLiteUserByToken(string token);
        public Task<string> GetUserByToken(string token);

        public Task<string> GetUserById(string id);
        public Task<bool> AddRegistration(Registration args);
        public Task<bool> CheckIsActive(HttpContext context);

    }
}

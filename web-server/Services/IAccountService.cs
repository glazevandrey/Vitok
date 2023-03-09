using System.Collections.Generic;
using web_server.DbContext;
using web_server.Models;

namespace web_server.Services
{
    public interface IAccountService
    {
        public User SaveAccountInfo(string args);

    }
}

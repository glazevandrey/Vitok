using web_server.Models;

namespace web_server.Services
{
    public interface ILessonsService
    {
        public User AddLessonsToUser(string[] args);
    }
}

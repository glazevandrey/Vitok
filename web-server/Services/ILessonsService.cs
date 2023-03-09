using System.Collections.Generic;
using web_server.DbContext;
using web_server.Models;

namespace web_server.Services
{
    public interface ILessonsService
    {
        public User AddLessonsToUser(string[] args);
        public List<RescheduledLessons> GetRescheduledLessons(string args);

    }
}

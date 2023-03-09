using web_server.Models;

namespace web_server.Services
{
    public interface ITutorService
    {
        public User AddTutor(string args);
        public User UpdateTutor(string args);
        public bool RemoveTutor(string args);
        public User AddTutorFreeDate(string args);
        public User AddTutorSchedule(string args);
        public User RemoveTutorSchedule(string args);
        public User RemoveTutorTime(string args);

    }
}

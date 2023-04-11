using System.Collections.Generic;
using System.Threading.Tasks;
using web_server.Models.DBModels;

namespace web_server.Services.Interfaces
{
    public interface ICourseService
    {
        public Task<string> SetNewCourse(string[] args);
        public Task<string> RemoveCourse(string args);
        public Task<string> EditCourse(string[] args);
        public Task<List<Course>> GetCourses();

    }
}

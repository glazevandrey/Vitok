using System;
using System.Linq;
using web_server.DbContext;
using web_server.Models;

namespace web_server.Services
{
    public class CourseService : ICourseService
    {
        public string SetNewCourse(string[] args)
        {
            var course = new Course();

            course.Title = args[0];
            course.Id = TestData.Courses.Last().Id + 1;
            course.Goal = TestData.Goals.FirstOrDefault(m => m.Id == Convert.ToInt32(args[1]));

            TestData.Courses.Add(course);

            return Newtonsoft.Json.JsonConvert.SerializeObject(course);
        }
    }
}

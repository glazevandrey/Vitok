using System;
using System.Linq;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class CourseService : ICourseService
    {
        public string EditCourse(string[] args)
        {
            var id = Convert.ToInt32(args[0]);
            var title = args[1];
            var goal = args[2];
            TestData.Courses.FirstOrDefault(m => m.Id == id).Title = title;
            TestData.Courses.FirstOrDefault(m => m.Id == id).Goals.Clear();
            TestData.Courses.FirstOrDefault(m => m.Id == id).Goals.Add(TestData.Goals.FirstOrDefault(m => m.Id == Convert.ToInt32(goal)));

            return Newtonsoft.Json.JsonConvert.SerializeObject("OK");
        }

        public string RemoveCourse(string args)
        {
            var id = Convert.ToInt32(args);
            var users = TestData.UserList;

            foreach (var item in users)
            {
                if (item.Courses == null)
                {
                    continue;
                }
                foreach (var course in item.Courses)
                {
                    if (course.Id == id)
                    {
                        return null;
                    }
                }
            }

            var rem = TestData.Courses.FirstOrDefault(m => m.Id == id);
            TestData.Courses.Remove(rem);

            return Newtonsoft.Json.JsonConvert.SerializeObject("OK");
        }

        public string SetNewCourse(string[] args)
        {
            var course = new Course();

            course.Title = args[0];
            course.Id = TestData.Courses.Last().Id + 1;
            course.Goals = TestData.Goals.Where(m => m.Id == Convert.ToInt32(args[1])).ToList();

            TestData.Courses.Add(course);

            return Newtonsoft.Json.JsonConvert.SerializeObject(course);
        }
    }
}

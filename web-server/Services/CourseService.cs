using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class CourseService : ICourseService
    {
        CourseRepository _courseRepository;
        UserRepository _userRepository;
        public CourseService(CourseRepository courseRepository, UserRepository userRepository)
        {
            _userRepository = userRepository;
            _courseRepository = courseRepository;
        }
        public async Task<List<Course>> GetCourses()
        {
            return await _courseRepository.GetAllCourses();
        }
        public async Task<string> EditCourse(string[] args)
        {
            var id = Convert.ToInt32(args[0]);
            var title = args[1];
            var goal = args[2];
            var course = await _courseRepository.GetCourseById(id);
            course.Title = title;
            course.Goal = new Goal();
            course.Goal = await _courseRepository.GetGoalById(Convert.ToInt32(goal));

            return Newtonsoft.Json.JsonConvert.SerializeObject("OK");
        }

        public async Task<string> RemoveCourse(string args)
        {
            var id = Convert.ToInt32(args);
            var users = await _userRepository.GetAll();
            //var users = TestData.UserList;

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

            await _courseRepository.RemoveCourse(id);

            return Newtonsoft.Json.JsonConvert.SerializeObject("OK");
        }

        public async Task<string> SetNewCourse(string[] args)
        {
            var course = new Course();
            var id = Convert.ToInt32(args[1]);
            course.Title = args[0];
            
            course.Goal = await _courseRepository.GetGoalById(id);

            await _courseRepository.AddCourse(course);

            return Newtonsoft.Json.JsonConvert.SerializeObject(course);
        }
    }
}

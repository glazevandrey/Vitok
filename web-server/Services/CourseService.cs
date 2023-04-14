using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web_server.Database.Repositories;
using web_server.DbContext;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;
using web_server.Services.Interfaces;

namespace web_server.Services
{
    public class CourseService : ICourseService
    {
        CourseRepository _courseRepository;
        UserRepository _userRepository;
        IMapper _mapper;
        public CourseService(IMapper mapper, CourseRepository courseRepository, UserRepository userRepository)
        {
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
        }
        public async Task<List<Course>> GetCourses()
        {
            var courses = await _courseRepository.GetAllCourses();
            return (_mapper.Map<List<Course>>(courses));
        }

        public async Task<string> EditCourse(string[] args)
        {
            var id = Convert.ToInt32(args[0]);
            var title = args[1];
            var goal = args[2];
            var course =  _mapper.Map<CourseDTO>(await _courseRepository.GetCourseById(id));
            course.Title = title;
            course.Goal = new GoalDTO();
            course.Goal =_mapper.Map<GoalDTO>(await _courseRepository.GetGoalById(Convert.ToInt32(goal)));
            await _courseRepository.Update(course);
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
            var course = new CourseDTO();
            var id = Convert.ToInt32(args[1]);
            course.Title = args[0];
            course.GoalId = id;
            //course.Goal =  _mapper.Map<GoalDTO>(await _courseRepository.GetGoalById(id));

            await _courseRepository.AddCourse(course);

            return Newtonsoft.Json.JsonConvert.SerializeObject(course);
        }
    }
}

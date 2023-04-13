using AutoMapper;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Tutor, TutorDTO>();
            CreateMap<Student, StudentDTO>();
            CreateMap<Manager, ManagerDTO>();

            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();

            CreateMap<TutorDTO, User>();
            CreateMap<StudentDTO, User>();
            CreateMap<TutorDTO, User>();


            CreateMap<TutorDTO, Tutor>();
            CreateMap<StudentDTO, Student>();
            CreateMap<ManagerDTO, Manager>();


            CreateMap<ScheduleDTO, Schedule>();
            CreateMap<Schedule, ScheduleDTO>();

            CreateMap<Course, CourseDTO>();
            CreateMap<CourseDTO, Course>();

            CreateMap<Chat, ChatDTO>();
            CreateMap<ChatDTO, Chat>();


        }
    }
}

using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using web_server.Models;
using web_server.Models.DBModels;
using web_server.Models.DTO;

namespace web_server
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Student, StudentDTO>();
            CreateMap<Manager, ManagerDTO>();

            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();

            CreateMap<TutorDTO, User>();
            CreateMap<StudentDTO, User>();
            CreateMap<TutorDTO, User>();
            CreateMap<TutorDTO, Tutor>();
            CreateMap<TutorDTO, Tutor>();
            CreateMap<Tutor, TutorDTO>();


            CreateMap<CourseDTO, CourseDTO>();            //            CreateMap<TutorCourse, CourseDTO>();
            CreateMap<CourseDTO, TutorCourse>().ForMember(m=>m.Course, m=>m.MapFrom(m=>m)).ForMember(m=>m.CourseId, m=>m.MapFrom(m=>m.Id)).ReverseMap();
            CreateMap<Course, TutorCourse>().ForMember(m => m.Course, m => m.MapFrom(m => m)).ForMember(m => m.CourseId, m => m.MapFrom(m => m.Id)).ReverseMap();

            // .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CourseId))
            // .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Course.Title))
            // .ReverseMap();
            //CreateMap<TutorCourse, Tutor>()
            //.ForMember(dest => dest.Courses, opt => opt.MapFrom(src => new List<CourseDTO> { src.Course }))
            //.ReverseMap();
            //CreateMap<Tutor, TutorDTO>()
            //           .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => src.Courses.Select(c => new TutorCourse { Course = c })))
            //           .ReverseMap();

            //CreateMap<TutorCourse, CourseDTO>().ForMember(m => m, d => d.MapFrom(m=>m.Course));
            //CreateMap<CourseDTO, TutorCourse>().ForMember(m => m., d => d.MapFrom(m => m));

            //CreateMap<CourseDTO, TutorCourse>().ForMember(m => m.Course.Title, s => s.MapFrom(f => f.Title)).ForMember(m => m.Course.GoalId, s => s.MapFrom(f => f.GoalId));

            // CreateMap<TutorDTO, Tutor>().ForMember(m=>m.Courses , ms=>ms.MapFrom(s=>s.Courses.First().Course));
            CreateMap<StudentDTO, Student>();
            CreateMap<ManagerDTO, Manager>();


            CreateMap<ScheduleDTO, Schedule>();
            CreateMap<Schedule, ScheduleDTO>();


            CreateMap<GoalDTO, Goal>();
            CreateMap<Goal, GoalDTO>();

            CreateMap<Registration, RegistrationDTO>();
            CreateMap<RegistrationDTO, Registration>();

            CreateMap<Course, CourseDTO>();
            CreateMap<CourseDTO, Course>();

            CreateMap<Chat, ChatDTO>();
            CreateMap<ChatDTO, Chat>();
            CreateMap<NotificationsDTO, Notifications>();
            CreateMap<Notifications, NotificationsDTO>();

            //CreateMap<TutorCourse, CourseDTO>()
            //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CourseId))
            //    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Course.Title));




            //CreateMap<TutorCourse, CourseDTO>()
            //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CourseId))
            //    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Course.Title))
            //    .ReverseMap();

            //CreateMap<TutorCourse, Tutor>()
            //    .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => new List<CourseDTO> { src.Course }))
            //    .ReverseMap();
            //CreateMap<TutorCourse, Tutor>()
            //.ForMember(dest => dest.Courses, opt => opt.MapFrom(src => new List<CourseDTO> { src.Course }));

        }
    }
}

using AutoMapper;
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

            CreateMap<NotificationTaskDTO, NotificationTask>();
            CreateMap<NotificationTask, NotificationTaskDTO>();


            CreateMap<TutorDTO, User>();
            CreateMap<StudentDTO, User>();
            CreateMap<TutorDTO, User>();
            CreateMap<TutorDTO, Tutor>();
            CreateMap<TutorDTO, Tutor>();
            CreateMap<Tutor, TutorDTO>();


            CreateMap<CourseDTO, CourseDTO>();            //            CreateMap<TutorCourse, CourseDTO>();
            CreateMap<CourseDTO, TutorCourse>().ForMember(m => m.Course, m => m.MapFrom(m => m)).ForMember(m => m.CourseId, m => m.MapFrom(m => m.Id)).ReverseMap();

            CreateMap<Course, TutorCourse>().ForMember(m => m.Course, m => m.MapFrom(m => m)).ForMember(m => m.CourseId, m => m.MapFrom(m => m.Id)).AfterMap((src, dest) => { dest.Course = null; }).ReverseMap();

      
            CreateMap<StudentDTO, Student>();
            CreateMap<ManagerDTO, Manager>();


            CreateMap<ScheduleDTO, Schedule>();//.ForMember(m=>m.CourseId, m=>m.MapFrom(m=>m.Course.Id)).AfterMap((src, dest) => { dest.Course = null; });
            CreateMap<Schedule, ScheduleDTO>().ForMember(m => m.CourseId, m => m.MapFrom(m => m.Course.Id)).AfterMap((src, dest) => { src.Course = null ; dest.Course = null; });//.ForMember(m=>m.CourseId, m=>m.MapFrom(m=>m.Course.Id));


            CreateMap<GoalDTO, Goal>();
            CreateMap<Goal, GoalDTO>();

            CreateMap<Registration, RegistrationDTO>().AfterMap((src, dest) => { dest.Course.Id = 0; dest.Course.Goal = null; });
            CreateMap<RegistrationDTO, Registration>();//.AfterMap((src, dest) => { dest.Course=  null;); ;

            CreateMap<Course, CourseDTO>();//.ForMember(m=>m.GoalId , m=>m.MapFrom(m=>m.Goal.Id)).ForMember(m=>m.Id , m=>m.MapFrom(m=>m.Id));
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

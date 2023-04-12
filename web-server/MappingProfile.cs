﻿using AutoMapper;
using System.Collections.Generic;
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

        }
    }
}
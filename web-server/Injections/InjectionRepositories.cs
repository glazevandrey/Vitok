using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using web_server.Database;
using web_server.Database.Repositories;
using web_server.Services;
using web_server.Services.Interfaces;

namespace web_server.Injection
{
    public static class InjectionRepositories
    {
        public static IServiceCollection AddCustomRepositories(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<UserRepository>();
            services.AddScoped<ContactsRepository>();
            services.AddScoped<CourseRepository>();
            services.AddScoped<NotificationRepository>();
            services.AddScoped<ScheduleRepository>();
            services.AddScoped<TariffRepositories>();

            return services;
        }
    }
}

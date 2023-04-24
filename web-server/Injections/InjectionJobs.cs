using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using web_server.Database;
using web_server.Database.Repositories;
using web_server.Quartz.Jobs;
using web_server.Quartz;
using web_server.Services;
using web_server.Services.Interfaces;

namespace web_server.Injection
{
    public static class InjectionJobs
    {
        public static IServiceCollection AddCustomJobs(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<MainParseJob>();

            return services;
        }
    }
}

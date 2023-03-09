using Microsoft.Extensions.DependencyInjection;
using web_server.Services;

namespace web_server.Injection
{
    public static class InjectionServices
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IJsonService, JsonService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ILessonsService, LessonsService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ITutorService, TutorService>();

            return services;
        }
    }
}

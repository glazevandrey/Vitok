using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using web_server.Database.Repositories;

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

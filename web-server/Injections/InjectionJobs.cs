using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using web_server.Quartz.Jobs;

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

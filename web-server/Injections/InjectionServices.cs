using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using web_server.Services;
using web_server.Services.Interfaces;

namespace web_server.Injection
{
    public static class InjectionServices
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IJsonService, JsonService>();
            services.AddScoped<IAuthService, AuthService>();
            //services.AddScoped<ICustomNotificationService, CustomNotificationService>(m => { var hub = m.GetService<IHubContext<NotifHub>>(); var not = m.GetService<NotificationRepository>(); var user = m.GetService<UserRepository>(); var mapper = m.GetService<IMapper>(); return new CustomNotificationService(mapper, not, user, hub); });
            services.AddScoped<ILessonsService, LessonsService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ITutorService, TutorService>();
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<ISenderService, SenderService>();

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();

            services.AddSingleton(mapper);

            return services;
        }
    }
}

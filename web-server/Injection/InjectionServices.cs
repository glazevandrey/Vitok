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
    public static class InjectionServices
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration config)
        {
            // ...
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            string connection = config.GetConnectionString("DefaultConnection");


            services.AddDbContext<DataContext>(options =>
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                options.UseSqlServer(connection);
                options.EnableSensitiveDataLogging();

            }, ServiceLifetime.Scoped);
            // services.AddDbContext<DataContext>(options => { options.UseSqlServer(connection)}, ServiceLifetime.Transient);

            services.AddScoped<UserRepository>();//(m => { var map = m.GetService<IMapper>(); return new UserRepository(map); });
            services.AddScoped<ContactsRepository>();
            services.AddScoped<CourseRepository>();
            services.AddScoped<NotificationRepository>();//(m => { return new NotificationRepository(); });
            services.AddScoped<ScheduleRepository>();//(m => {  return new ScheduleRepository(); });
            services.AddScoped<TariffRepositories>();


            services.AddScoped<IJsonService, JsonService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICustomNotificationService, CustomNotificationService>(m => { var hub = m.GetService<IHubContext<NotifHub>>(); var not = m.GetService<NotificationRepository>(); var user = m.GetService<UserRepository>()  ; return new CustomNotificationService(not, user, hub); });

            services.AddScoped<ILessonsService, LessonsService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ITutorService, TutorService>();
            services.AddScoped<IStatisticsService, StatisticsService>();


            services.AddScoped<ISenderService, SenderService>();

            return services;
        }
    }
}

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz.Logging;
using System;
using web_server.Database;
using web_server.Database.Repositories;
using web_server.Injection;
using web_server.Quartz;

namespace web_server
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSwaggerGen();
            services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true;
                o.MaximumReceiveMessageSize = 31457280; // bytes
            });
            services.AddControllers();


            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

          .AddCookie(options =>
          {
              options.LoginPath = "/Account/Login";
          })
          .AddJwtBearer(options =>
          {
              options.SaveToken = true;
              options.RequireHttpsMetadata = false;
              options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
              {
                  ValidateIssuer = true,
                  ValidateAudience = true,
                  ValidAudience = Program.web_app_ip + "/",
                  ValidIssuer = "http://localhost:23571/",
                  IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes("YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv"))
                  ,
                  ValidateLifetime = true,
                  ClockSkew = TimeSpan.Zero
              };
          });

            string connection = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<DataContext>(options =>
            {
                options.UseNpgsql(connection);
                options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
                options.EnableSensitiveDataLogging();

            }, ServiceLifetime.Scoped);

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            services.AddCustomRepositories(Configuration);
            services.AddCustomServices(Configuration);

            services.AddScoped<IQuartzService, QuartzService>();
            services.AddCustomJobs(Configuration);
            var serviceProvider = services.BuildServiceProvider();
            NotifHub.Configure(serviceProvider);
            QuartzStartup.Start(serviceProvider, Configuration); // запуск выполнения планировщика задач 


            //services.AddSingleton<IHostedService, NotificationBackgroundService>();

            //            services.AddHostedService<NotificationBackgroundService>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserRepository userRepository)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // 1. Смягчаем политику для локальной разработки (SecurePolicy.SameAsRequest)
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax, // Lax вместо Strict, чтобы кука ходила между портами localhost
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = env.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always
            });

            app.UseRouting();

            // 2. CORS ДОЛЖЕН БЫТЬ ТУТ (До аутентификации)
            app.UseCors(x => x
                .WithOrigins(Program.web_app_ip, "http://localhost:5173", "http://localhost:3000") // Добавь порты твоего реакта
                .AllowCredentials()
                .AllowAnyMethod()
                .AllowAnyHeader());

            // 3. КАСТОМНЫЙ МИДЛВАР ТУТ (Вытаскиваем токен ДО выполнения аутентификации)
            app.Use(async (context, next) =>
            {
                var token = context.Request.Cookies[".AspNetCore.Application.Id"];

                if (!string.IsNullOrEmpty(token))
                {
                    if (context.Request.Headers["Authorization"].Count > 0)
                    {
                        context.Request.Headers["Authorization"] = "Bearer " + token;
                    }
                    else
                    {
                        context.Request.Headers.Add("Authorization", "Bearer " + token);
                    }
                }
                else if (context.Request.Query.ContainsKey("token"))
                {
                    if (context.Request.Headers["Authorization"].Count > 0)
                    {
                        context.Request.Headers["Authorization"] = "Bearer " + context.Request.Query["token"];
                    }
                    else
                    {
                        context.Request.Headers.Add("Authorization", "Bearer " + context.Request.Query["token"]);
                    }
                }

                await next();
            });

            app.UseSwagger();
            app.UseSwaggerUI();

            // 4. АУТЕНТИФИКАЦИЯ ТУТ (Теперь она увидит подложенный Bearer токен!)
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chatHub");
                endpoints.MapHub<NotifHub>("/notifHub");
            });
        }
    }
}

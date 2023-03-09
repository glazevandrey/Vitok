using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using web_server.DbContext;
using web_server.Injection;
using web_server.Services;

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
            services.AddCors();
            services.AddCustomServices();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSwaggerGen();
            services.AddSignalR();
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
              options.AccessDeniedPath = "/Home/Error";
          })
          .AddJwtBearer(options =>
          {
              options.SaveToken = true;
              options.RequireHttpsMetadata = false;
              options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
              {
                  ValidateIssuer = true,
                  ValidateAudience = true,
                  ValidAudience = "http://localhost:23571/",
                  ValidIssuer = "http://localhost:35944/",
                  IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes("YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv"))
                  ,
                  ValidateLifetime = true,
                  ClockSkew = TimeSpan.Zero
              };
          });
            services.AddHostedService<NotificationBackgroundService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always
            });
            app.UseRouting();
            app.UseSwagger();

            app.UseSwaggerUI();
            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                var token = context.Request.Cookies[".AspNetCore.Application.Id"];


                if (!string.IsNullOrEmpty(token))
                {
                    context.Request.Headers.Add("Authorization", "Bearer " + token);
                }
                else
                {
                    token = context.Request.Headers[".AspNetCore.Application.Id"];
                    if (token != null && token != "")
                    {
                        context.Request.Headers.Add("Authorization", "Bearer " + token);
                    }
                    if (context.Request.Query.ContainsKey("token"))
                    {
                        context.Request.Headers.Add("Authorization", "Bearer " + TestData.UserList.FirstOrDefault(m => m.UserId == Convert.ToInt32(context.Request.Query["token"])).ActiveToken);
                    }
                }




                await next();
            });
            //      app.UseMvc();
            app.UseAuthentication();
            app.UseCors(x => x
                    //.WithOrigins("https://localhost:44340")// путь к нашему SPA клиенту
                    .WithOrigins("http://localhost:23571")// путь к нашему SPA клиенту
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSignalR(routes =>
            {
                routes.MapHub<NotifHub>("/notifHub");
                routes.MapHub<ChatHub>("/chatHub");
            });

        }
    }
}

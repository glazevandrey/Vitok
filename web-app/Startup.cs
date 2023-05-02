using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using web_app.Services;
using web_server.Services;
using web_server.Services.Interfaces;

namespace web_app
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        private readonly IWebHostEnvironment _env;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<IWebHostEnvironment>(_env);

            //   services.AddSignalR();
            // services.AddSession();
            services.AddScoped<IRequestService, RequestService>();
            services.AddScoped<IJsonService, JsonService>();

            services.AddControllersWithViews();
            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });
            //  services.AddSignalR();
#pragma warning disable CS0618 // "CompatibilityVersion.Version_2_2" является устаревшим: 'This CompatibilityVersion value is obsolete. The recommended alternatives are Version_3_0 or later.'
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
#pragma warning restore CS0618 // "CompatibilityVersion.Version_2_2" является устаревшим: 'This CompatibilityVersion value is obsolete. The recommended alternatives are Version_3_0 or later.'


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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCookiePolicy();

            app.UseAuthentication();
            //app.UseSignalR(routes =>
            //{
            //    routes.MapHub<Class>("/chatHub");
            //});
            app.Use(async (context, next) =>
            {
                var token = context.Request.Cookies[".AspNetCore.Application.Id"];
                if (!string.IsNullOrEmpty(token))
                    context.Request.Headers.Add("Authorization", "Bearer " + token);

                await next();
            });
            app.UseEndpoints(routes =>
            {

                routes.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

            });
        }
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace web_server
{
    public class Program
    {
        public static Dictionary<int, Timer> Timers = new Dictionary<int, Timer>();
        public static string web_app_ip = "";
        public static bool BackInAir = false;
        public static JsonSerializerSettings settings = new JsonSerializerSettings();

        public static void Main(string[] args)
        {
            web_app_ip = "http://localhost:23571";
            settings.Converters.Add(new UserConverter());


            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://localhost:23382");
                });
    }
}

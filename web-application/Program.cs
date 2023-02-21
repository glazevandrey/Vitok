using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Globalization;

namespace web_application
{
  
    public class Program
    {
        public static string server_ip = "http://localhost:35944/";
        public static void Main(string[] args)
        {
            
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

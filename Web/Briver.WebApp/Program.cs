using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Briver.Framework;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Briver.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SystemContext.Initialize(new App());
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();
        }
    }

    internal class App : Application
    {
        protected override void Configure(ConfigurationBuilder config)
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "Config");

            Directory.CreateDirectory(dir);
            foreach (var file in Directory.EnumerateFiles(dir, "*.json"))
            {
                config.AddJsonFile(file, false, true);
            }

        }

    }
}

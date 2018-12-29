using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var dir = Path.Combine(this.BaseDirectory, "Config");
            if (!Directory.Exists(dir))
            {
                return;
            }

            foreach (var file in Directory.EnumerateFiles(dir, "*.json"))
            {
                config.AddJsonFile(file, false, true);
            }

        }

        protected override Information LoadInformation()
        {
            return new Information
            {
                Name = "Briver.WebApp",
                Version = "1.0",
                DisplayName = "自定义网站应用"
            };
        }
    }
}

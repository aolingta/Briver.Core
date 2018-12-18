using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Briver.Framework;
using Briver.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

[assembly: CompositionSupported]

namespace Briver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SystemContext.Initialize(new App());

            var builder = WebHost.CreateDefaultBuilder(args);

            builder.UseKestrel(opt =>
            {
                opt.ListenLocalhost(12345);
            });
            builder.UseStartup<Startup>();

            var host = builder.Build();

            // 如果运行于调试模式或者带有参数，则直接运行，否则以服务方式运行
            if (Debugger.IsAttached || args.Contains("--console"))
            {
                host.Run();
            }
            else
            {
                host.RunAsService();
            }
        }

    }

    internal class App : Application
    {
        protected override void Configure(ConfigurationBuilder config)
        {
            var dir = Path.Combine(this.BaseDirectory, "Config");
            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*.json"))
                {
                    config.AddJsonFile(file);
                }
            }
        }
    }
}

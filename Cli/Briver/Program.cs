using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Briver.Aspect;
using Briver.Commands;
using Briver.Framework;
using Briver.Logging;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace Briver
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var app = new App();
                SystemContext.Initialize(app);

                app.Aspect().Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
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

        internal void Execute(string[] args)
        {
            var cli = new CommandLineApplication();
            foreach (var cmd in SystemContext.GetExports<ICommand>().Where(it => it.Parent == null))
            {
                cli.Command(cmd.Name, cmd.Execute);
            }

            if (args.Length == 0)
            {
                cli.ShowHelp();
                return;
            }

            cli.Execute(args);
        }

    }

    [Composition(Priority = 1, Description = "记录系统启动的日志")]
    internal class AppInitiator : ISystemInitialization
    {
        void ISystemInitialization.Execute()
        {
            Logger.Info("系统已启动");
        }
    }

    internal class LogInterception : Interception
    {
        public override void Intercept(AspectContext context, AspectDelegate proceed)
        {
            Logger.Trace($"{nameof(LogInterception)}:开始执行方法{context.Method.Name}");
            proceed.Invoke();
            Logger.Trace($"{nameof(LogInterception)}:完成执行方法{context.Method.Name}");
        }
    }
}

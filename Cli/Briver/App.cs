﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Briver.Aspect;
using Briver.Commands;
using Briver.Framework;
using Briver.Logging;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace Briver
{
    internal class App : Application
    {
        /// <summary>
        /// 通过读取文件加载信息
        /// </summary>
        /// <returns></returns>
        protected override Information LoadInformation()
        {
            var information_file = "Briver.json";
            string BuildErrorText()
            {
                return $"运行目录“{this.BaseDirectory}”下不存在文件“{information_file}”或者此文件不是有效的json格式，请确保此文件存在并且使用{Encoding.UTF8.EncodingName}编码。";
            }
            var path = Path.Combine(this.BaseDirectory, information_file);
            if (!File.Exists(path))
            {
                throw new FrameworkException(ExceptionLevel.Fatal, BuildErrorText());
            }
            var json = File.ReadAllText(path, Encoding.UTF8);
            var info = JsonConvert.DeserializeObject<Information>(json);
            if (info == null)
            {
                throw new FrameworkException(ExceptionLevel.Fatal, BuildErrorText());
            }
            return info;
        }

        internal void Execute(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("请输入命令:");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                args = line.ParseCommandLineArguments();
            }

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
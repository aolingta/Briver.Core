using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Briver.Aspect;
using Briver.Commands;
using Briver.Framework;
using Briver.Logging;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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
                Console.WriteLine($"{app.Name} {app.Version}");

                RunCommand(args);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static void RunCommand(string[] args)
        {
            try
            {
                while (true)
                {
                    if (args == null || args.Length == 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("{0}请输入命令{0}{0}", new string('-', 6));
                        var line = Console.ReadLine();
                        if (line == null) //用户按了CTRL+C(取消)
                        {
                            break;
                        }
                        args = line.ParseCommandLineArguments();
                        if (args.Length == 0)
                        {
                            continue;
                        }
                    }

                    var cli = new CommandLineApplication();
                    try
                    {
                        foreach (var cmd in SystemContext.GetExports<ICommand>().Where(it => it.Parent == null))
                        {
                            cli.Command(cmd.Name, cmd.Execute);
                        }

                        cli.Execute(args);
                    }
                    catch (CommandParsingException)
                    {
                        Console.WriteLine("错误：输入命令的命令无效");
                        cli.ShowHelp();
                    }
                    finally
                    {
                        args = null;//清理参数
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("发生了异常", ex.ToString());
                throw;
            }

        }
    }



}

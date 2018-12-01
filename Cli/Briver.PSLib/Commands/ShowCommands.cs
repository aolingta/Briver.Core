using System;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using Briver.Framework;

namespace Briver.Commands
{
    [Cmdlet(VerbsCommon.Show, "Commands")]
    public class ShowCommands : Cmdlet
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            SystemContext.Initialize(new App());

            Console.WriteLine(SystemContext.Application.BaseDirectory);

            Console.WriteLine("请使用以下命令：");

            int index = 0;
            var query = from cmd in SystemContext.GetExports<ICommand>()
                        let attr = cmd.GetType().GetCustomAttribute<CmdletAttribute>()
                        where attr != null
                        orderby attr.VerbName, attr.NounName
                        select cmd;
            foreach (var cmd in query)
            {
                var attr = cmd.GetType().GetCustomAttribute<CmdletAttribute>(false);
                if (attr == null) { continue; }

                index++;
                Console.WriteLine($"{index}) {attr.VerbName}-{attr.NounName}");
            }
        }

    }
}

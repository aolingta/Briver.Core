using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Briver.Framework;
using Microsoft.Extensions.CommandLineUtils;

namespace Briver.Commands
{
    public interface ICommand : IComposition
    {
        Type Parent { get; }
        string Name { get; }
        void Execute(CommandLineApplication app);
    }

    public abstract class Command : ICommand
    {
        public virtual Type Parent => null;

        const string suffix = "Command";
        public virtual string Name
        {
            get
            {
                var name = this.GetType().Name;
                var index = name.LastIndexOf(suffix, StringComparison.OrdinalIgnoreCase);
                if (index > 0)
                {
                    name = name.Substring(0, index);
                }
                return name;
            }
        }

        public void Execute(CommandLineApplication app)
        {
            app.HelpOption("--help|-h");
            foreach (var cmd in SystemContext.GetExports<ICommand>().Where(it => it.Parent == this.GetType()))
            {
                app.Command(cmd.Name, cmd.Execute);
            }

            this.Initialize(app);

            app.OnExecute(this.Execute);
        }

        protected abstract void Initialize(CommandLineApplication app);
        protected abstract int Execute();
    }

}

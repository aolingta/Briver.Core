using System;
using System.Collections.Generic;
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
                app.Aspect().Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }


}

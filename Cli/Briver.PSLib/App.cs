using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Briver.Framework;
using Microsoft.Extensions.Configuration;

namespace Briver
{
    internal class App : Application
    {
        public override string BaseDirectory { get; }
            = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        protected override Information LoadInformation()
        {
            return new Information
            {
                Name = "Briver.PSLib",
                Version = "1.0",
                DisplayName = ""
            };
        }
    }
}

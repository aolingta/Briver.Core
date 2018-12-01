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
    }
}

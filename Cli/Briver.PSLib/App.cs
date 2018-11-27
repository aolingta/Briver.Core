using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Briver.Framework;
using Microsoft.Extensions.Configuration;

namespace Briver
{
    class App : Application
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
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Briver.Framework;
using Briver.Logging;
using Microsoft.Extensions.Configuration;

namespace Briver.Plugins
{
    public class VirtualDisk : ISystemInitialization
    {
        public class Config
        {
            public class Mapping
            {
                public string Driver { get; set; }
                public string Directory { get; set; }
            }

            public List<Mapping> Mappings { get; } = new List<Mapping>();
        }

        void ISystemInitialization.Execute()
        {
            var config = SystemContext.Configuration.GetSection(nameof(VirtualDisk)).Get<Config>();
            foreach (var item in config.Mappings)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "subst",
                        Arguments = $"{item.Driver} {item.Directory}",
                        UseShellExecute = false,

                    }).WaitForExit();

                    Logger.Info($"映射盘符“{item.Driver}”成功");
                }
                catch (Exception ex)
                {
                    Logger.Warn($"映射盘符“{item.Driver}”失败", ex.ToString());
                }
            }
        }
    }


}

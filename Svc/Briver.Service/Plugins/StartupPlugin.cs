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
    public class StartupPlugin : ISystemInitialization
    {
        public class Config
        {
            public class Job
            {
                public string Name { get; set; }
                public string Command { get; set; }
                public string Argument { get; set; }
            }

            public List<Job> Jobs { get; } = new List<Job>();
        }

        void ISystemInitialization.Execute()
        {
            var config = SystemContext.Configuration.GetSection(nameof(StartupPlugin)).Get<Config>();
            foreach (var job in config.Jobs)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = job.Command,
                        Arguments = job.Argument,
                        UseShellExecute = false,

                    }).WaitForExit();

                    Logger.Info($"执行任务“{job.Name}”成功");
                }
                catch (Exception ex)
                {
                    Logger.Warn($"执行任务“{job.Name}”失败", ex.ToString());
                }
            }
        }
    }


}

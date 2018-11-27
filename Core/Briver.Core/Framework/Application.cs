using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Briver.Framework;
using Microsoft.Extensions.Configuration;

namespace Briver.Framework
{
    /// <summary>
    /// 应用程序，需要在业务系统中提供具体的实现
    /// </summary>
    public abstract class Application
    {
        /// <summary>
        /// 执行配置
        /// </summary>
        /// <param name="config"></param>
        protected internal abstract void Configure(ConfigurationBuilder config);

        /// <summary>
        /// 加载系统要用到的程序集
        /// </summary>
        /// <returns></returns>
        protected internal virtual IEnumerable<Assembly> LoadAssemblies()
        {
            var assemblies = new Dictionary<string, Assembly>();

            foreach (var assem in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assem.GetName().Name.ToLower();
                if (!assemblies.ContainsKey(name))//可能已经加载了同名的程序集
                {
                    assemblies.Add(name, assem);
                }
            }

            foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll"))
            {
                var name = Path.GetFileNameWithoutExtension(file).ToLower();
                if (!assemblies.ContainsKey(name))
                {
                    try
                    {
                        assemblies.Add(name, Assembly.Load(AssemblyName.GetAssemblyName(file)));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"加载程序集{name}失败：{ex.Message}");
                    }
                }
            }

            return assemblies.Values;
        }

    }



}

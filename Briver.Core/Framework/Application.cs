using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Briver.Framework
{
    /// <summary>
    /// 应用程序，需要在业务系统中提供具体的实现
    /// </summary>
    public abstract class Application
    {
        /// <summary>
        /// 系统信息
        /// </summary>
        protected class Information
        {
            /// <summary>
            /// 系统名称（不能为空）
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 系统版本（不能为空且必须符合System.Version要求）
            /// </summary>
            public string Version { get; set; }

            /// <summary>
            /// 系统显示名称（不能为空）
            /// </summary>
            public string DisplayName { get; set; }

            /// <summary>
            /// 系统说明
            /// </summary>
            public string Description { get; set; }

            internal Information ValidateProperties()
            {
                void ThrowException(string message)
                {
                    throw new FrameworkException(ExceptionLevel.Fatal, $"验证“{nameof(Information)}失败，{message}");
                }
                if (string.IsNullOrWhiteSpace(this.Name))
                {
                    ThrowException($"属性“{nameof(Name)}”为空");
                }
                if (string.IsNullOrWhiteSpace(this.Version))
                {
                    ThrowException($"属性“{nameof(Version)}”为空");
                }
                if (!System.Version.TryParse(this.Version, out var version))
                {
                    ThrowException($"属性“{nameof(Version)}”不是有效的{typeof(Version).FullName}对象");
                }
                if (string.IsNullOrWhiteSpace(this.DisplayName))
                {
                    ThrowException($"属性“{nameof(DisplayName)}”为空");
                }

                return this;
            }
        }

        private Lazy<Information> _information;

        /// <summary>
        /// 系统名称
        /// </summary>
        public string Name => _information.Value.Name;

        /// <summary>
        /// 系统版本
        /// </summary>
        public string Version => _information.Value.Version;

        /// <summary>
        /// 系统显示名称
        /// </summary>
        public string DisplayName => _information.Value.DisplayName;

        /// <summary>
        /// 系统说明
        /// </summary>
        public string Description => _information.Value.Description;

        /// <summary>
        /// 基准目录（执行程序所在的目录）
        /// </summary>
        public virtual string BaseDirectory { get; } = AppContext.BaseDirectory;

        /// <summary>
        /// 用户目录（存放用户相关的配置等）
        /// </summary>
        public virtual string UserDirectory => this.BaseDirectory;

        /// <summary>
        /// 工作目录（存放日志、临时数据等）
        /// </summary>
        public virtual string WorkDirectory => this.UserDirectory;


        public Application()
        {
            _information = new Lazy<Information>(() => this.LoadInformation().ValidateProperties());
        }

        /// <summary>
        /// 加载基本信息
        /// </summary>
        /// <returns></returns>
        protected abstract Information LoadInformation();

        /// <summary>
        /// 执行配置
        /// </summary>
        /// <param name="config"></param>
        protected internal virtual void Configure(ConfigurationBuilder config)
        {
            void LoadConfig(string dir)
            {
                if (Directory.Exists(dir))
                {
                    foreach (var file in Directory.EnumerateFiles(dir, "*.json"))
                    {
                        config.AddJsonFile(file, false, true);
                    }
                }
            }

            const string ConfigDir = "Config";
            var baseConfig = Path.GetFullPath(Path.Combine(this.BaseDirectory, ConfigDir));
            var userConfig = Path.GetFullPath(Path.Combine(this.UserDirectory, ConfigDir));

            LoadConfig(baseConfig);
            if (!string.Equals(baseConfig, userConfig, StringComparison.OrdinalIgnoreCase))
            {
                LoadConfig(userConfig);
            }
        }

        /// <summary>
        /// 加载指定目录下的程序集
        /// </summary>
        /// <param name="directories">目录列表</param>
        /// <returns>返回当前应用程序域中所有已加载的程序集</returns>
        public static IDictionary<string, Assembly> LoadAssemblies(IEnumerable<string> directories)
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

            foreach (var path in directories ?? Enumerable.Empty<string>())
            {
                if (!Directory.Exists(path)) { continue; }

                foreach (var file in Directory.EnumerateFiles(path, "*.dll"))
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
            }

            return assemblies;
        }


        /// <summary>
        /// 加载系统要用到的程序集
        /// </summary>
        /// <returns></returns>
        protected internal virtual IEnumerable<Assembly> LoadAssemblies()
        {
            return LoadAssemblies(new string[] { this.BaseDirectory }).Values;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

}

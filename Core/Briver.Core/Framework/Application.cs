using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Briver.Framework;
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
            public string Name { get; set; }
            public string Version { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
        }

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

        private Lazy<Information> _information;

        /// <summary>
        /// 系统名称
        /// </summary>
        public string Name
        {
            get
            {
                var name = _information.Value.Name;
                if (string.IsNullOrEmpty(name))
                {
                    throw new FrameworkException(ExceptionLevel.Fatal, $"文件“{information_file}”中的“{nameof(Name)}”为空");
                }
                return name;
            }
        }

        /// <summary>
        /// 系统版本
        /// </summary>
        public string Version
        {
            get
            {
                var version = _information.Value.Version;
                if (string.IsNullOrEmpty(version))
                {
                    throw new FrameworkException(ExceptionLevel.Fatal, $"文件“{information_file}”中的“{nameof(Version)}”为空");
                }
                return version;
            }
        }

        /// <summary>
        /// 系统显示名称
        /// </summary>
        public string DisplayName
        {
            get
            {
                var displayName = _information.Value.DisplayName;
                if (string.IsNullOrEmpty(displayName))
                {
                    throw new FrameworkException(ExceptionLevel.Fatal, $"文件“{information_file}”中的“{nameof(DisplayName)}”为空");
                }
                return displayName;
            }
        }

        /// <summary>
        /// 系统说明
        /// </summary>
        public string Description => _information.Value.Description;

        public Application()
        {
            _information = new Lazy<Information>(LoadInformation);
        }

        private const string information_file = "Application.json";
        protected virtual Information LoadInformation()
        {
            string BuildErrorText()
            {
                return $"运行目录“{this.BaseDirectory}”下不存在文件“{information_file}”或者此文件不是有效的json格式，请确保此文件存在并且使用{Encoding.UTF8.EncodingName}编码。";
            }
            var path = Path.Combine(this.BaseDirectory, information_file);
            if (!File.Exists(path))
            {
                throw new FrameworkException(ExceptionLevel.Fatal, BuildErrorText());
            }
            var json = File.ReadAllText(path, Encoding.UTF8);
            var info = JsonConvert.DeserializeObject<Information>(json);
            if (info == null)
            {
                throw new FrameworkException(ExceptionLevel.Fatal, BuildErrorText());
            }
            return info;
        }

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

            foreach (var file in Directory.EnumerateFiles(this.BaseDirectory, "*.dll"))
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

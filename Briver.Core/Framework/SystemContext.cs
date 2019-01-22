using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace Briver.Framework
{
    /// <summary>
    /// 系统运行环境
    /// </summary>
    public static class SystemContext
    {
        private static readonly object @lock = new object();
        private static bool _initialized = false;
        private static Application _application = null;
        private static CompositionContext _composition = null;
        private static IConfigurationRoot _configuration = null;

        /// <summary>
        /// 初始化，在系统启动时调用
        /// </summary>
        public static void Initialize(Application application)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            lock (@lock)
            {
                if (_application != null)
                {
                    Debug.WriteLine($"{nameof(SystemContext)}已经初始化多次");
                    return;
                }

                _application = application;
                _configuration = _application.BuildConfiguration();
                _composition = _application.BuildComposition();

                _initialized = true;
            }

            foreach (var plugin in GetExports<ISystemInitialization>())
            {
                plugin.Execute();
            }
        }

        private static IConfigurationRoot BuildConfiguration(this Application application)
        {
            var config = new ConfigurationBuilder();
            application.Configure(config);
            return config.Build();
        }

        private static CompositionContext BuildComposition(this Application application)
        {
            var @interface = typeof(IComposition);
            var convention = new ConventionBuilder();
            convention.ForTypesDerivedFrom(@interface)
                .ExportInterfaces(type => @interface.IsAssignableFrom(type)).Shared();
            var assemblies = application.LoadAssemblies()
                .Where(assem => assem.GetCustomAttribute<CompositionSupportedAttribute>() != null);
            var builder = new ContainerConfiguration().WithAssemblies(assemblies, convention);

            return new DelegatingCompositionContext(builder.CreateContainer());
        }

        private static void EnsureState()
        {
            lock (@lock)
            {
                if (!_initialized)
                {
                    throw new InvalidOperationException($"{nameof(SystemContext)}尚未初始化");
                }
            }
        }


        /// <summary>
        /// 配置信息
        /// </summary>
        public static IConfigurationRoot Configuration
        {
            get
            {
                EnsureState();
                return _configuration;
            }
        }

        /// <summary>
        /// 应用程序
        /// </summary>
        public static Application Application
        {
            get
            {
                EnsureState();
                return _application;
            }
        }


        /// <summary>
        /// 获取导出列表
        /// </summary>
        /// <typeparam name="T">导出的类型（继承IComposition的接口）</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetExports<T>() where T : IComposition
        {
            EnsureState();
            return _composition.GetExports<T>();
        }

        /// <summary>
        /// 获取优先级最高的导出
        /// </summary>
        /// <typeparam name="T">导出的类型（继承IComposition的接口）</typeparam>
        /// <returns></returns>
        public static T GetHeadExport<T>() where T : IComposition
        {
            return GetExports<T>().FirstOrDefault();
        }


        /// <summary>
        /// 执行导入
        /// </summary>
        /// <param name="target">目标对象</param>
        public static void SatisfyImports(object target)
        {
            EnsureState();
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            _composition.SatisfyImports(target);
        }
    }

}

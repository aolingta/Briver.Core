using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;

namespace Briver.Framework
{
    /// <summary>
    /// 可组合对象
    /// </summary>
    public interface IComposition
    {
    }

    /// <summary>
    /// 元数据
    /// </summary>
    public interface ICompositionMetadata
    {
        /// <summary>
        /// 优先级
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 显示名称
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// 说明
        /// </summary>
        string Description { get; }
    }

    /// <summary>
    /// 元数据信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CompositionAttribute : Attribute, ICompositionMetadata
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }

    }

    /// <summary>
    /// 声明程序集支持组合
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class CompositionSupportedAttribute : Attribute
    {
    }


    /// <summary>
    /// 系统初始化插件
    /// </summary>
    public interface ISystemInitialization : IComposition
    {
        /// <summary>
        /// 加载时要执行的动作
        /// </summary>
        void Execute();
    }

    /// <summary>
    /// 扩展方法
    /// </summary>
    public static partial class CompositionExtensions
    {
        private class CompositionMetadata : ICompositionMetadata
        {
            public static readonly CompositionMetadata Empty = new CompositionMetadata(null, 0, null, null);

            public CompositionMetadata(string name, int priority, string displayName, string description)
            {
                this.Name = name ?? string.Empty;
                this.Priority = priority;
                this.DisplayName = displayName ?? string.Empty;
                this.Description = description ?? string.Empty;
            }

            public CompositionMetadata(Type type, CompositionAttribute attribute)
            {
                this.Name = attribute.Name ?? type.Name;
                this.Priority = attribute.Priority;
                this.DisplayName = attribute.DisplayName ?? this.Name;
                this.Description = attribute.Description ?? string.Empty;
            }

            public string Name { get; }
            public int Priority { get; }
            public string DisplayName { get; }
            public string Description { get; }
        }

        private static readonly ConcurrentDictionary<Type, ICompositionMetadata> _metadatas = new ConcurrentDictionary<Type, ICompositionMetadata>();

        /// <summary>
        /// 获取组件的元数据
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static ICompositionMetadata GetCompositionMetadata(this IComposition @this)
        {
            if (@this == null)
            {
                return CompositionMetadata.Empty;
            }

            return _metadatas.GetOrAdd(@this.GetType(), type =>
            {
                var attribute = type.GetCustomAttribute<CompositionAttribute>(false);
                if (attribute != null)
                {
                    return new CompositionMetadata(type, attribute);
                }

                return new CompositionMetadata(type.Name, 0, type.Name, string.Empty);
            });

        }
    }


    /// <summary>
    /// 实现对导出进行排序的功能
    /// </summary>
    internal class DelegatingCompositionContext : CompositionContext
    {
        private CompositionContext _context;
        public DelegatingCompositionContext(CompositionContext context)
        {
            _context = context;
        }

        private static readonly string IsImportManyName = typeof(CompositionContext).GetField("ImportManyImportMetadataConstraintName", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null).ToString();// "IsImportMany";

        private class PriorityComparer : IComparer<IComposition>
        {
            public int Compare(IComposition x, IComposition y)
            {
                var xx = x.GetCompositionMetadata();
                var yy = y.GetCompositionMetadata();
                return yy.Priority.CompareTo(xx.Priority);
            }
        }

        public override bool TryGetExport(CompositionContract contract, out object export)
        {
            var importMany = false;
            if (contract.MetadataConstraints != null)
            {
                importMany = contract.MetadataConstraints
                    .Any(it => it.Key == IsImportManyName && it.Value is bool isImportMany && isImportMany);
            }

            if (!importMany) // 不管是导出一个还是多个，始终按照导出多个的方式处理
            {
                var constraints = new Dictionary<string, object> { { IsImportManyName, true } };
                if (contract.MetadataConstraints != null)
                {
                    foreach (var item in contract.MetadataConstraints)
                    {
                        constraints.Add(item.Key, item.Value);
                    }
                }
                contract = new CompositionContract(contract.ContractType.MakeArrayType(), contract.ContractName, constraints);
            }

            if (!_context.TryGetExport(contract, out export))
            {
                return false;
            }

            if (export is IComposition[] array)
            {
                if (array.Length > 1)
                {
                    Array.Sort(array, new PriorityComparer());
                }
            }
            else
            {
                throw new CompositionFailedException($"不支持的类型：{contract.ContractType.FullName}");
            }

            export = array;
            if (!importMany)
            {
                export = array.FirstOrDefault();
            }
            return true;
        }
    }

}

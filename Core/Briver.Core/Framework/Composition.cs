using System;
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
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

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
                var xx = x?.GetCompositionMetadata().Priority;
                var yy = y?.GetCompositionMetadata().Priority;
                return (yy.GetValueOrDefault(0)).CompareTo(xx.GetValueOrDefault(0));
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

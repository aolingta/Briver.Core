using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Briver.Framework
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static partial class Extensions
    {
        private class CompositionMetadata : ICompositionMetadata
        {
            public static CompositionMetadata Empty = new CompositionMetadata(null);

            public CompositionMetadata(object target)
            {
                this.DisplayName = target?.GetType().Name ?? string.Empty;
            }

            public int Priority => 0;

            public string Description => string.Empty;

            public string DisplayName { get; }
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
                ICompositionMetadata metadata = type.GetCustomAttribute<CompositionAttribute>(false);
                return metadata ?? new CompositionMetadata(@this);
            });

        }
    }
}
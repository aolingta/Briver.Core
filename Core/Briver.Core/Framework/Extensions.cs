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
        private class DefaultCompositionMetadata : ICompositionMetadata
        {
            public static readonly Lazy<ICompositionMetadata> Instance = new Lazy<ICompositionMetadata>(() => new DefaultCompositionMetadata());

            public int Priority => 0;

            public string Description => string.Empty;
        }

        private static readonly ConcurrentDictionary<RuntimeTypeHandle, ICompositionMetadata> _metadatas = new ConcurrentDictionary<RuntimeTypeHandle, ICompositionMetadata>();

        /// <summary>
        /// 获取组件的元数据
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static ICompositionMetadata GetCompositionMetadata(this IComposition @this)
        {
            if (@this == null) { return DefaultCompositionMetadata.Instance.Value; }
            return _metadatas.GetOrAdd(@this.GetType().TypeHandle,
                handle => Type.GetTypeFromHandle(handle).GetCustomAttribute<CompositionAttribute>(false) ?? DefaultCompositionMetadata.Instance.Value);
        }
    }
}

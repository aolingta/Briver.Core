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
}
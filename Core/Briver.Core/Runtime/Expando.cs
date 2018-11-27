using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Briver.Runtime
{
    /// <summary>
    /// 为对象提供扩展的属性
    /// </summary>
    public static partial class ExpandoExtension
    {
        /// <summary>
        /// 支持动态化的字典对象提供者
        /// </summary>
        private class DynamicProperties : DynamicObject
        {
            ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

            /// <summary>
            /// 获取指定名称的值
            /// </summary>
            /// <param name="name">名称</param>
            /// <returns></returns>
            public object GetValue(string name)
            {
                if (String.IsNullOrEmpty(name))
                {
                    throw new ArgumentException($"参数“{nameof(name)}”的值不能为空。");
                }
                _cache.TryGetValue(name, out object value);
                return value;
            }

            /// <summary>
            /// 设置指定名称的值
            /// </summary>
            /// <param name="name">名称</param>
            /// <param name="value">值</param>
            public void SetValue(string name, object value)
            {
                if (String.IsNullOrEmpty(name))
                {
                    throw new ArgumentException($"参数“{nameof(name)}”的值不能为空。");
                }
                _cache[name] = value;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = GetValue(binder.Name);
                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                SetValue(binder.Name, value);
                return true;
            }

        }

        private static readonly ConditionalWeakTable<object, DynamicProperties> _cache = new ConditionalWeakTable<object, DynamicProperties>();

        private static DynamicProperties GetProperties(object @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }
            if (@this is ValueType)
            {
                throw new NotSupportedException($"参数“{nameof(@this)}”不能为值类型的对象");
            }

            return _cache.GetOrCreateValue(@this);
        }

        /// <summary>
        /// 获取动态扩展对象（通常用于存储额外的属性等）
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static dynamic Expando(this object @this)
        {
            return GetProperties(@this);
        }

        /// <summary>
        /// 获取动态扩展属性值
        /// </summary>
        /// <typeparam name="T">扩展属性值的类型</typeparam>
        /// <param name="this"></param>
        /// <param name="name">扩展属性的名称</param>
        /// <returns></returns>
        public static T Expando<T>(this object @this, string name)
        {
            return (T)GetProperties(@this).GetValue(name);
        }

        /// <summary>
        /// 设置动态扩展属性值
        /// </summary>
        /// <typeparam name="T">扩展属性值的类型</typeparam>
        /// <param name="this"></param>
        /// <param name="name">扩展属性的名称</param>
        /// <param name="value">扩展属性的值</param>
        public static void Expando<T>(this object @this, string name, T value)
        {
            GetProperties(@this).SetValue(name, value);
        }
    }

}

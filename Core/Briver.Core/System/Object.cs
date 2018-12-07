using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Briver.Framework;

namespace System
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// 判断当前对象是否指定的类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool Is<T>(this object @this)
        {
            return @this is T;
        }

        /// <summary>
        /// 是否空对象
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsNull(this object @this)
        {
            return @this == null;
        }

        /// <summary>
        /// 将当前对象（强制）转型成指定的类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T As<T>(this object @this) where T : class
        {
            return @this as T;
        }


        /// <summary>
        /// 将当前对象转换成指定的类型，如果转换失败，则返回默认值
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="this">当前对象</param>
        /// <param name="default">如果转换失败，将要使用的默认值</param>
        /// <returns></returns>
        public static T To<T>(this object @this, T @default = default)
        {
            if (@this != null)
            {
                if (@this is T)
                {
                    return (T)@this;
                }

                var type = typeof(T);
                TypeConverter converter;

                converter = TypeDescriptor.GetConverter(type);
                if (converter.CanConvertFrom(@this.GetType()))
                {
                    return (T)converter.ConvertFrom(@this);
                }

                converter = TypeDescriptor.GetConverter(@this);
                if (converter.CanConvertTo(type))
                {
                    return (T)converter.ConvertTo(@this, type);
                }
            }

            return @default;
        }


        /// <summary>
        /// 为当前对象执行一个动作
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="this">当前对象</param>
        /// <param name="action">要执行的动作</param>
        /// <returns></returns>
        public static T Do<T>(this T @this, Action<T> action)
        {
            if (@this != null && action != null)
            {
                action(@this);
            }
            return @this;
        }
        /// <summary>
        /// 为当前对象执行一个动作
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="this">当前对象</param>
        /// <param name="predicate">前置条件</param>
        /// <param name="action">要执行的动作</param>
        /// <returns></returns>
        public static T Do<T>(this T @this, Predicate<T> predicate, Action<T> action)
        {
            if (@this != null && predicate != null && action != null && predicate(@this))
            {
                action(@this);
            }
            return @this;
        }

    }

}

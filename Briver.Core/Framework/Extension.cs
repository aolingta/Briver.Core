using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Briver.Framework
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// 获取指定类型的配置元素
        /// </summary>
        /// <typeparam name="T">配置元素的类型</typeparam>
        /// <param name="this"></param>
        /// <param name="key">配置元素的主键</param>
        /// <returns></returns>
        public static T Retrieve<T>(this IConfiguration @this, string key)
        {
            if (@this != null)
            {
                return @this.GetSection(key).Get<T>();
            }
            return default;
        }
    }
}

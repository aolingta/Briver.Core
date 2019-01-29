using System;
using System.Text.RegularExpressions;

namespace Briver.Extension
{
    public static partial class StringMethods
    {
        /// <summary>
        /// 是否空字符串
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string @this)
        {
            return String.IsNullOrEmpty(@this);
        }

        /// <summary>
        /// 是否匹配正则表达式
        /// </summary>
        /// <param name="this"></param>
        /// <param name="regex">正则表达式</param>
        /// <param name="options">正则表达式的选项</param>
        /// <returns></returns>
        public static bool IsMatch(this string @this, string regex, RegexOptions options = RegexOptions.Singleline | RegexOptions.ExplicitCapture)
        {
            if (@this == null || String.IsNullOrEmpty(regex))
            {
                return false;
            }

            return Regex.IsMatch(@this, regex, options);
        }
    }
}

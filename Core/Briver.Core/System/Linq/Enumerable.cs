using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace System.Linq
{
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// 对集合中的每个元素执行指定的方法
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="this">集合</param>
        /// <param name="action">要执行的方法</param>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            if (@this == null || action == null)
            {
                goto RETURN;
            }
            foreach (var item in @this)
            {
                action(item);
            }

        RETURN:
            return @this;
        }

        /// <summary>
        /// 将集合转换成表
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="this">集合</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> @this)
        {
            var table = new DataTable();
            var properties = TypeDescriptor.GetProperties(typeof(T));
            for (int i = 0; i < properties.Count; i++)
            {
                var p = properties[i];
                table.Columns.Add(new DataColumn(p.Name, p.PropertyType));
            }

            if (@this == null)
            {
                goto RETURN;
            }
            foreach (var item in @this)
            {
                var row = table.NewRow();
                for (int i = 0; i < properties.Count; i++)
                {
                    row[i] = properties[i].GetValue(item);
                }
                table.Rows.Add(row);
            }

        RETURN:
            return table;
        }
    }
}

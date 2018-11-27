using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Briver.Http
{
    /// <summary>
    /// 将对象序列化后作为Body提交
    /// </summary>
    public class ObjectContent : StringContent
    {
        public ObjectContent(object model)
            : base(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
        {

        }
    }

    /// <summary>
    /// 以FormUrlEncodedContent的方式提交
    /// </summary>
    public class FormContent : FormUrlEncodedContent
    {
        public FormContent(object model) : base(Build(model))
        {
#if DEBUG
            string body = ReadAsStringAsync().GetAwaiter().GetResult();
#endif
        }

        private static IEnumerable<KeyValuePair<string, string>> Build(object model)
        {
            if (model == null)
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }

            if (model is IEnumerable<KeyValuePair<string, string>> strings)
            {
                return strings;
            }

            if (model is IEnumerable<KeyValuePair<string, object>> objects)
            {
                return objects.Select(pair => new KeyValuePair<string, string>(pair.Key, GetString(pair.Value)));
            }

            return TypeDescriptor.GetProperties(model).Cast<PropertyDescriptor>()
                .Select(property => new KeyValuePair<string, string>(property.Name, GetString(property.GetValue(model))));
        }

        private static string GetString(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var cvt = TypeDescriptor.GetConverter(value);
            if (cvt.CanConvertFrom(typeof(string)))
            {
                return cvt.ConvertToString(value);
            }
            return JsonConvert.SerializeObject(value);
        }
    }
}

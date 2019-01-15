using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Briver.Framework;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Briver.Web
{
    /// <summary>
    /// API上下文
    /// </summary>
    public class ApiContext
    {
        /// <summary>
        /// Http请求
        /// </summary>
        public HttpRequest Request { get; }

        /// <summary>
        /// 返回数据的分页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 返回数据的分页编号
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// API上下文
        /// </summary>
        /// <param name="request">Http请求</param>
        public ApiContext(HttpRequest request)
        {
            this.Request = request;
        }

        /// <summary>
        /// 读取内容，返回字符串
        /// </summary>
        /// <returns></returns>
        public string ReadContent()
        {
            using (var reader = new StreamReader(this.Request.Body))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// 读取内容，返回Json反序列化对象
        /// </summary>
        /// <typeparam name="T">反序列化对象类型</typeparam>
        /// <returns></returns>
        public T ReadContent<T>()
        {
            return JsonConvert.DeserializeObject<T>(ReadContent());
        }
    }

    /// <summary>
    /// API处理器
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IApiHanlder : IComposition
    {
        Task<ApiResult> ProcessAsync(ApiContext context);
    }

    /// <summary>
    /// API处理器
    /// </summary>
    public abstract class ApiHandler : IApiHanlder
    {
        private static readonly Lazy<IReadOnlyDictionary<string, IApiHanlder>> _handlers
            = new Lazy<IReadOnlyDictionary<string, IApiHanlder>>(() =>
            {
                var suffix = "handler";
                var suffixLength = suffix.Length;
                var dict = new Dictionary<string, IApiHanlder>();
                foreach (var handler in SystemContext.GetExports<IApiHanlder>())
                {
                    var name = handler.GetCompositionMetadata().Name.ToLower();
                    dict.Add(name, handler);

                    if (name.EndsWith(suffix))
                    {
                        name = name.Substring(0, name.Length - suffixLength);
                        dict.Add(name, handler);
                    }
                }
                return new ReadOnlyDictionary<string, IApiHanlder>(dict);
            });

        /// <summary>
        /// 所有的API处理器
        /// </summary>
        public static bool TryGet(string name, out IApiHanlder hanlder)
        {
            return _handlers.Value.TryGetValue(name.ToLower(), out hanlder);
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="context">API上下文</param>
        /// <returns></returns>
        public abstract Task<ApiResult> ProcessAsync(ApiContext context);
    }
}

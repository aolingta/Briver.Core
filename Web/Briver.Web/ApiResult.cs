using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Briver.Web
{
    /// <summary>
    /// 接口响应结果
    /// </summary>
    public class ApiResult : IActionResult
    {
        /// <summary>
        ///  响应状态
        /// </summary>
        public class ResponseStatus
        {
            /// <summary>
            /// 代码
            /// </summary>
            public int Code { get; }

            /// <summary>
            /// 消息
            /// </summary>
            public string Message { get; }

            /// <summary>
            /// 响应状态
            /// </summary>
            /// <param name="code">代码</param>
            /// <param name="message">消息</param>
            internal ResponseStatus(int code, string message)
            {
                this.Code = code;
                this.Message = message;
            }
        }

        /// <summary>
        /// 响应状态
        /// </summary>
        public ResponseStatus Status { get; }

        /// <summary>
        /// 数据
        /// </summary>
        public object Content { get; }

        /// <summary>
        /// 接口响应结果
        /// </summary>
        /// <param name="code">代码</param>
        /// <param name="content">数据</param>
        /// <param name="message">消息</param>
        public ApiResult(int code, object content = null, string message = null)
        {
            this.Status = new ResponseStatus(code, message);
            this.Content = content;
        }

        /// <summary>
        /// 内部使用OkObjectResult生成响应
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        async Task IActionResult.ExecuteResultAsync(ActionContext context)
        {
            await new OkObjectResult(this).ExecuteResultAsync(context);
        }
    }

}

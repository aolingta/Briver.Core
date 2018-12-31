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
        /// 状态
        /// </summary>
        public bool Status { get; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// 接口响应结果
        /// </summary>
        /// <param name="status">状态</param>
        public ApiResult(bool status)
        {
            this.Status = status;
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

        /// <summary>
        /// 版本不支持
        /// </summary>
        public static ApiResult VersionUnsupported => new ApiResult(false) { Message = "请使用更高版本的接口" };
    }

}

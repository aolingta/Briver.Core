using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Briver.Framework;
using Briver.Logging;
using Briver.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Briver.WebApp.Api
{
    /// <summary>
    /// 通用的API入口
    /// </summary>
    [ApiController]
    [Route("api")]
    [Route("api/v{version:ApiVersion}")]
    public class ApiController : ControllerBase
    {
        /// <summary>
        /// 异步调用指定的API处理器
        /// </summary>
        /// <param name="apiName">API名称</param>
        /// <param name="pageSize">数据分页大小</param>
        /// <param name="pageIndex">数据分页编号</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        [Route("{apiName}")]
        public async Task<IActionResult> InvokeAsync(string apiName,
            int pageSize = 50, int pageIndex = 0)
        {
            Logger.Info($"收到请求{Request.Path}");
            if (ApiHandler.TryGet(apiName, out var handler))
            {
                var context = new ApiContext(Request)
                {
                    PageSize = pageSize,
                    PageIndex = pageIndex,
                };
                return await handler.ProcessAsync(context);
            }
            return NotFound();
        }
    }

}
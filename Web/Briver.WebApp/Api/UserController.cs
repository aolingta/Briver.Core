using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Briver.Aspect;
using Briver.Framework;
using Briver.Logging;
using Briver.Web;
using Briver.WebApp.Api.Handlers;
using Briver.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Briver.WebApp.Api.Controllers
{

    /// <summary>
    /// 用户控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:ApiVersion}/[controller]")]
    public abstract class UserController : ApiController
    {
        protected const string ControllerName = "User";

        /// <summary>
        /// 列出所有用户
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(List))]
        public virtual async Task<IActionResult> List()
        {
            if (ApiHandler.TryGet(nameof(UserHandler), out var handler))
            {
                return await handler.ProcessAsync(new ApiContext(this.Request) { });
            }
            return NotFound();
        }

        /// <summary>
        /// 查询状态
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(Status))]
        public virtual IActionResult Status()
        {
            return ApiResult.VersionUnsupported;
        }

    }

    [ApiVersion("1.0")]
    [ControllerName(ControllerName)]
    public class UserControllerV1 : UserController
    {
    }


    [ApiVersion("2.0")]
    [ControllerName(ControllerName)]
    public class UserControllerV2 : UserControllerV1
    {
        public override Task<IActionResult> List()
        {
            return Task.FromResult<IActionResult>(new ApiResult(true)
            {
                Content = new UserModel[] {
                    new UserModel { Name = "chenyj", Time = DateTime.Now },
                    new UserModel { Name = "陈勇江" }
                }
            });
        }

        public override IActionResult Status()
        {
            return new ApiResult(true) { Message = "OK" };
        }
    }

}
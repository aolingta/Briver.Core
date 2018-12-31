using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Briver.Aspect;
using Briver.Framework;
using Briver.Logging;
using Briver.Web;
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
    public abstract class UserController : Controller
    {
        protected const string ControllerName = "User";

        /// <summary>
        /// 列出所有用户
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(List))]
        public virtual IActionResult List()
        {
            var repository = new UserRepository();
            var count = 10000;
            var watch = Stopwatch.StartNew();
            Parallel.For(0, count, ii =>
            {
                var ss = repository.Aspect().Load("", out DateTime time);
            });
            watch.Stop();
            Logger.Info($"执行{count}次动态调用，用时{watch.Elapsed.TotalMilliseconds}毫秒");

            repository.Aspect().Conn = "abc";
            var conn = (string)repository.Aspect().Conn;
            repository.Aspect().Save();

            return new ApiResult(true) { Content = repository.Aspect().GetUsers() };
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
        public override IActionResult List()
        {
            return new ApiResult(true)
            {
                Content = new UserModel[] {
                    new UserModel { Name = "chenyj", Time = DateTime.Now },
                    new UserModel { Name = "陈勇江" }
                }
            };
        }

        public override IActionResult Status()
        {
            return new ApiResult(true) { Message = "OK" };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Log(Priority = 1)]
    public class UserRepository
    {
        public UserModel[] GetUsers()
        {
            return new UserModel[] { new UserModel { Name = "chenyj", Time = DateTime.Now } };
        }

        public void Save() { }

        public (bool, string) Load(string source, out DateTime time)
        {
            time = DateTime.Now;
            return (true, string.Empty);
        }

        public string Conn { get; set; }
    }

    public class LogAttribute : InterceptionAttribute
    {
        public override void Intercept(AspectContext context, AspectDelegate proceed)
        {
            //Logger.Info($"{nameof(LogInterceptionAttribute)}调用方法{context.Method}");
            proceed.Invoke();
        }
    }

    [Composition(Priority = 3)]
    internal class Log : Interception
    {
        public override void Intercept(AspectContext context, AspectDelegate proceed)
        {
            //Logger.Info($"{nameof(LogInterception)}调用方法{context.Method}");
            proceed.Invoke();
        }
    }
}
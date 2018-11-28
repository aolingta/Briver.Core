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

namespace Briver.WebApp.Api
{
    [ApiController]
    [Route("api/user")]
    [Route("api/v1/user")]
    public class UserController : Controller
    {
        [HttpGet("list")]
        public IActionResult List()
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

            return new ApiResult(0, repository.Aspect().GetUsers());
        }
    }

    [Route("api/v2/user")]
    public class UserController2 : UserController
    {

    }

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
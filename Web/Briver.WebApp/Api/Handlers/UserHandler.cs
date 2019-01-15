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

namespace Briver.WebApp.Api.Handlers
{

    /// <summary>
    /// 用户控制器
    /// </summary>
    public class UserHandler : ApiHandler
    {
        public override Task<ApiResult> ProcessAsync(ApiContext context)
        {
            var repository = new UserRepository();
            return Task.FromResult(new ApiResult(true)
            {
                Content = repository.Aspect().GetUsers()
            });
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
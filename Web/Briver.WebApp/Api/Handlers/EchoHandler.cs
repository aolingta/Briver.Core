using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Briver.Framework;
using Briver.Web;

namespace Briver.WebApp.Api.Handlers
{
    public class EchoHandler : ApiHandler
    {
        public override Task<ApiResult> ProcessAsync(ApiContext context)
        {
            return Task.FromResult(new ApiResult(true) { Content = context.ReadContent() });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Briver.Framework;
using Briver.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Briver.WebApp.Api
{
    [ApiController]
    [Route("api")]
    [Route("api/v{version:ApiVersion}")]
    public class ApiController : ControllerBase
    {
        [HttpGet("{handler}")]
        [HttpPost("{handler}")]
        public async Task<IActionResult> InvokeAsync(string handler, int pageSize = 50, int pageIndex = 0)
        {
            if (ApiHandler.TryGet(handler, out var apiHandler))
            {
                var context = new ApiContext(this.Request)
                {
                    PageSize = pageSize,
                    PageIndex = pageIndex,
                };
                return await apiHandler.ProcessAsync(context);
            }
            return NotFound();
        }
    }

}
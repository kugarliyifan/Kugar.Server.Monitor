using Kugar.Core.Web.ActionResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Kugar.Server.MonitorServer.Areas.API.Controllers
{
    [Area("CollectorApi")]
    //[Authorize(AuthenticationSchemes = "CollectorApi")]
    //[FromBodyJson 
    [ApiExplorerSettings(GroupName = "CollectorApi")]
    [Produces("application/json")]
    public class BaseController : Controller, IAsyncActionFilter
    {
        
    }
}

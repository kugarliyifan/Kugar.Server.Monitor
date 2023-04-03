using Kugar.Core.Web.ActionResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Kugar.Core.ExtMethod;

namespace Kugar.Server.MonitorServer.Areas.API.Controllers
{
    [Route("Import/Project/{ProjectId}/{controller}/{action}")]
    public class ProjectBaseController : BaseController
    {
        [NonAction]
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new ModelStateValidActionResult();
            }

            if (context.Result == null)
            {
                if (context.HttpContext.GetRouteData().Values.TryGetValue("ProjectId", out var projectIdStr))
                {
                    if (Guid.TryParseExact(projectIdStr.ToStringEx(),"N",out var projectId))
                    {
                        if (!projectId.IsEmpty())
                        {
                            CurrentProjectId = projectId;
                        }
                    } 
                }

                if (CurrentProjectId.IsEmpty())
                {
                    context.Result = new NotFoundResult();
                }

                if (context.Result == null)
                {
                    var ret = await next();

                    context.Result = ret.Result;
                }

            } 

        }

        protected Guid CurrentProjectId {  get; private set; }
    }
}

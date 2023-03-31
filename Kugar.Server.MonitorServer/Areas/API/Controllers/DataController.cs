using Kugar.Core.ExtMethod;
using Kugar.Core.Web;
using Kugar.Server.MonitorServer.Services.EventData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Kugar.Server.MonitorServer.Areas.API.Controllers
{
    public class DataController : ProjectBaseController
    {
        [Route("CollectorApi/Project/Data/UploadData/[ProjectId]")]
        [HttpPost] 
        public async Task<IActionResult> UploadData(
            [FromQuery]string typeId,
            [FromRoute(Name = "ProjectId")]Guid projectId,
            [FromBody]JObject body,
            [FromServices] EventDataService service=null
            )
        {
            var eventDt = body.GetDateTime("eventDt", DateTime.Now);
            var serverIP = body.GetString("serverIp");
            body.Remove("eventDt");
            body.Remove("typeId");
            body.Remove("serverIP");

            var ret=await service.AddEventData(
                typeId,
                projectId,
                serverIP,
                body,
                eventDt
            );

            return Json(ret);
        }
    }
}

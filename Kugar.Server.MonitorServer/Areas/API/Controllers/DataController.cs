﻿using Kugar.Core.ExtMethod;
using Kugar.Core.Web;
using Kugar.Server.MonitorServer.Services.EventData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Kugar.Server.MonitorServer.Areas.API.Controllers
{
    public class DataController : ProjectBaseController
    {
        //[Route("/CollectorApi/Project/Data/UploadData/{ProjectId}")]
        [HttpPost] 
        public async Task<IActionResult> UploadData(
            [FromQuery]string typeId, 
            [FromBody]JObject body,
            [FromServices] EventDataService service=null
            )
        {
            var eventDt = body.GetDateTime("eventDt", DateTime.Now,StringComparison.CurrentCultureIgnoreCase);
            var serverIP = body.GetString("serverIp",null,StringComparison.CurrentCultureIgnoreCase);
            //body.Remove("eventDt");
            //body.Remove("typeId");
            //body.Remove("serverIP");

            var ret=await service.AddEventData(
                typeId,
                CurrentProjectId,
                serverIP??this.Request.GetClienIP(),
                body,
                eventDt
            );

            return Json(ret);
        }
    }
}

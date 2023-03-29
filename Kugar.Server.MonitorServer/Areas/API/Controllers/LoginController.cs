using System.ComponentModel.DataAnnotations;
using Kugar.Core.ExtMethod;
using Kugar.Core.Web;
using Kugar.Core.Web.Controllers;
using Kugar.Server.MonitorServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kugar.Server.MonitorServer.Areas.API.Controllers
{
    public class LoginController : BaseController
    {
        [Route("CollectorApi/Users/Login")]
        [HttpPost,FromBodyJson]
        public async Task<IActionResult> Login(
            [Required,StringLength(20,MinimumLength = 5)]string loginUserName,
            [Required]string password,
            [FromServices] AdminService service=null
            )
        {
            var ret=await service.LoginByName(loginUserName, password);
            var token = "";

            if (ret)
            {
                token = this.BuildJWtToken(ret.ReturnData.AdminId.ToStringEx(), password);
            }

            return Json(ret.Cast(token,""));
        }
     
    }
}

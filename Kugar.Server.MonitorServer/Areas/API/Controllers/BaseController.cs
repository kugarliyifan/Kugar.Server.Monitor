using Microsoft.AspNetCore.Mvc;

namespace Kugar.Server.MonitorServer.Areas.API.Controllers
{
    public class BaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

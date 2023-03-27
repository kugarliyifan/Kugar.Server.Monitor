using Kugar.Core.BaseStruct;
using Kugar.Core.Web.Authentications;

namespace Kugar.Server.MonitorServer.Areas.API.Helpers
{
    public class ProjectApiLoginService:IWebJWTLoginService
    {
        public Task<ResultReturn<string>> Login(HttpContext context, string userName, string password, IEnumerable<KeyValuePair<string, string>> values = null,
            bool isNeedEncoding = false)
        {
            throw new NotImplementedException();
        }
    }
}

using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.Core.Web.Authentications;
using Kugar.Server.MonitorServer.Services;

namespace Kugar.Server.MonitorServer.Areas.API.Helpers
{
    public class ProjectApiLoginService:IWebJWTLoginService
    {
        public async Task<ResultReturn<string>> Login(HttpContext context, string userName, string password, IEnumerable<KeyValuePair<string, string>> values = null,
            bool isNeedEncoding = false)
        {
            var service = ServiceProviderServiceExtensions.GetService<AdminService>(context.RequestServices);

            var adminId = Guid.Parse(userName);

            var ret = await service.LoginById(adminId, password);

            return ret.Cast<string>((ret.ReturnData?.AdminId).ToStringEx(),"");
        }
    }
}

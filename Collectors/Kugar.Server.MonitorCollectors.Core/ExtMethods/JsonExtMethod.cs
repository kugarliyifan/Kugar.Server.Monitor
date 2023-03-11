using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Kugar.Server.MonitorCollectors.Core.ExtMethods
{
    public static class JsonExtMethod
    {
        public static Guid GetGuid(this JObject json, string propertyName)
        {
            if (json.TryGetValue(propertyName, out var str))
            {
                var s = (string)str;

                if (Guid.TryParse(s,out var g))
                {
                    return g;
                }
            }

            return Guid.Empty;
        }
    }
}

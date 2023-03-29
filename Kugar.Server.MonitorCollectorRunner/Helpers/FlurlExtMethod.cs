using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Kugar.Core.BaseStruct;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.Server.MonitorCollectorRunner.Helpers
{
    public static class FlurlExtMethod
    {
        public static async Task<JObject> GetJsonObject(this IFlurlResponse response)
        {
            await using var stream = await response.GetStreamAsync();
            using var sr = new StreamReader(stream);
            using var jr = new JsonTextReader(sr);

            var json = await JObject.ReadFromAsync(new JsonTextReader(new StreamReader(stream)));

            return (JObject)json;
        }

        public static async Task<ResultReturn<T?>> GetResultReturn<T>(this IFlurlResponse response)
        {
            var json = await response.GetJsonObject();

            return json.ToObject<ResultReturn<T>>();
        }

        public static async Task<ResultReturn?> GetResultReturn(this IFlurlResponse response)
        {
            var json = await response.GetJsonObject();

            return json.ToObject<ResultReturn>();
        }
    }
}

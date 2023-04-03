using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Kugar.Core.BaseStruct;
using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;
using Kugar.Core.Services;
using Kugar.Server.MonitorServer.Data;
using Newtonsoft.Json.Linq;

namespace Kugar.Server.MonitorServer.Services.EventData
{
    public class EventDataService : BaseService
    {
        private static ConcurrentDictionary<string,object> _eventNameCache = new ConcurrentDictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
        private static string _prefix;
        private static string _orgId = "";
        private static string _orgName = "";
   
        static EventDataService()
        {
            _prefix = CustomConfigManager.Default["InfluxDb:Prefix"];

            var api=GlobalProvider.Provider.GetService<InfluxDBClient>()
                .GetOrganizationsApi();

            _orgName = CustomConfigManager.Default["InfluxDb:Org"];

            var orgs = api.FindOrganizationsAsync().Result;

            var org= orgs.FirstOrDefault(x => x.Name == _orgName);

            if (org==null)
            {
                org = api.CreateOrganizationAsync(_orgName).Result;
            }

            _orgId = org.Id;
        }

        public EventDataService(InfluxDBClient influxDbClient, MonitorDbContext dbContext) : base(influxDbClient, dbContext)
        {
        }

        public async Task<ResultReturn> AddEventData(
            string eventDataType,
            Guid projectId,
            string ipAddress,
            JObject data,
            DateTime eventDt
            )
        {
            var server = await Server.GetServerByIpAddress(ipAddress);

            if (server==null)
            {
                return new FailResultReturn("服务器不存在");
            }

            var projectIdStr = projectId.ToStringEx();

            var pointToWrite = PointData.Measurement(eventDataType)
                .Timestamp(eventDt, WritePrecision.Ms)
                .Tag("ServerId", server.ServerId.ToStringEx())
                .Tag("ProjectId", projectIdStr)
                .Field("d",false);

            foreach (var property in data.Properties())
            {
                if (property.Name.CompareTo("EventDt",true) ||
                    property.Name.CompareTo("ServerId", true) ||
                    property.Name.CompareTo("ProjectId", true) ||
                    property.Name.CompareTo("typeId", true) 
                    )
                {
                    continue;
                }

                var value = property.Value/*.ToPrimitiveValue()*/;

                if (value.Type== JTokenType.Boolean)
                {
                    pointToWrite = pointToWrite.Tag(property.Name, (bool)value ? "1" : "0");
                }
                else
                {
                    pointToWrite = pointToWrite.Tag(property.Name, value.ToStringEx());
                }

                //pointToWrite=pointToWrite.Tag(property.Name, value.Switch(value.ToStringEx()).Case(true, "1").Case(false, "0").Result);
            }

            try
            {
                var bucketName = _prefix + eventDataType;

                _eventNameCache.GetOrAdd(bucketName, name =>
                {
                    var api = InfluxDbClient.GetBucketsApi();

                    try
                    {
                        var b = api.FindBucketByNameAsync(name).Result;

                        if (b == null)
                        {
                            api.CreateBucketAsync(name,
                                new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, 
                                    (long?)TimeSpan
                                    .FromDays(CustomConfigManager.Default.GetValue<int>("InfluxDb:ExpireDays"))
                                .TotalSeconds),
                                    _orgId)
                                .Wait();
                        }
                    }
                    catch (Exception e)
                    {
                        return 1;
                    }


                    return 1;
                });
                
                var writer = InfluxDbClient.GetWriteApiAsync();

                await writer.WritePointAsync(pointToWrite, bucketName, _orgName);
            }
            catch (Exception e)
            {
                return new FailResultReturn(e);
            }


            return SuccessResultReturn.Default;

        }

        public virtual ServerService Server { set; get; }
    }
}

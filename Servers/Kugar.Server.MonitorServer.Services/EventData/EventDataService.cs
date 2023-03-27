using System;
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
using Kugar.Server.MonitorServer.Data;
using Newtonsoft.Json.Linq;

namespace Kugar.Server.MonitorServer.Services.EventData
{
    public class EventDataService:BaseService
    {
        private static HashSet<string> _eventNameCache = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        public EventDataService(InfluxDBClient influxDbClient, MonitorDbContext dbContext) : base(influxDbClient, dbContext)
        {
        }

        public async Task<ResultReturn> AddEventData(
            string eventDataType,
            Guid projectId,
            JObject data,
            DateTime eventDt
            )
        {
            var pointToWrite = PointData.Measurement(eventDataType)
                .Timestamp(eventDt, WritePrecision.Ms)
                .Tag("ProjectId",projectId.ToStringEx());

            foreach (var property in data.Properties())
            {
                var value = property.Value.ToPrimitiveValue();

                pointToWrite.Tag(property.Name, value.Switch(value.ToStringEx()).Case(true,"1").Case(false,"0").Result);
            }

            try
            {
                if (!_eventNameCache.Contains(eventDataType))
                {
                    var api = InfluxDbClient.GetBucketsApi();

                    var b = await api.FindBucketByNameAsync(eventDataType);

                    if (b != null)
                    {
                        _eventNameCache.Add(eventDataType);
                    }
                    else
                    {
                        await api.CreateBucketAsync(eventDataType,
                            new BucketRetentionRules(everySeconds: (long?)TimeSpan
                                .FromDays(CustomConfigManager.Default["InfluxDb:ExpireDays"].ToInt()).TotalSeconds),
                            CustomConfigManager.Default["InfluxDb:Org"]);

                        _eventNameCache.Add(eventDataType);
                    }
                }
                 
                var writer = InfluxDbClient.GetWriteApiAsync();

                await writer.WritePointAsync(pointToWrite, eventDataType, CustomConfigManager.Default["InfluxDb:Org"]);
            }
            catch (Exception e)
            {
                return new FailResultReturn(e);
            }


            return SuccessResultReturn.Default;

        }
    }
}

using InfluxDB.Client;
using Kugar.Server.MonitorServer.Data;

namespace Kugar.Server.MonitorServer.Services
{
    public abstract class BaseService
    {
        protected BaseService(InfluxDBClient influxDbClient, MonitorDbContext dbContext)
        {
            InfluxDbClient = influxDbClient;
            DbContext = dbContext;
        }

        protected InfluxDBClient InfluxDbClient { set; get; }


        protected MonitorDbContext DbContext { set; get; }
    }
}
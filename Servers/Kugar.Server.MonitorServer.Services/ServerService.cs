using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.Client;
using Kugar.Server.MonitorServer.Data;
using Kugar.Server.MonitorServer.Data.DbEntities;

namespace Kugar.Server.MonitorServer.Services
{
    public class ServerService:BaseService
    {
        public ServerService(InfluxDBClient influxDbClient, MonitorDbContext dbContext) : base(influxDbClient, dbContext)
        {
        }

        public async Task<base_Server?> GetServerByIpAddress(string ipAddress)
        {
            return await DbContext.base_Server.Where(x => x.IpAddress == ipAddress).FirstAsync();

        }
    }
}

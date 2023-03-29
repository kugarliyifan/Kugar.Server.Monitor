using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.Client;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.Server.MonitorServer.Data;
using Kugar.Server.MonitorServer.Data.DbEntities;

namespace Kugar.Server.MonitorServer.Services
{
    public class AdminService:BaseService
    {
        public AdminService(InfluxDBClient influxDbClient, MonitorDbContext dbContext) : base(influxDbClient, dbContext)
        {
        }

        public async Task<ResultReturn<base_Admin>> LoginByName(string username, string password)
        {
            var user = await DbContext.base_Admin
                .Where(x =>!x.IsDeleted && x.State==0 && x.LoginName == username && x.Password == password.MD5_32(true).MD5_32(true))
                .FirstAsync();

            return new SuccessResultReturn<base_Admin>(user);
        }

        public async Task<ResultReturn<base_Admin>> LoginById(Guid adminId, string password)
        {
            var user = await DbContext.base_Admin
                .Where(x => !x.IsDeleted && x.State == 0 && x.AdminId == adminId && x.Password == password.MD5_32(true).MD5_32(true))
                .FirstAsync();

            return new SuccessResultReturn<base_Admin>(user);
        }
    }
}

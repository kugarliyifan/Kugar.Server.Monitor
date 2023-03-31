using FreeSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.FreeSql.Repositories;
using Kugar.Server.MonitorServer.Data.DbEntities;

namespace Kugar.Server.MonitorServer.Data
{
    public class MonitorDbContext : DatabaseContextBase
    {
        public MonitorDbContext(IFreeSql freesql, UnitOfWorkManager uowm) : base(freesql, uowm)
        {
        }

        /// <summary>
        /// 管理员表
        /// </summary>
        public virtual IBaseRepository<base_Admin, Guid> base_Admin { set; get; }

        /// <summary>
        /// 项目表
        /// </summary>
        public virtual IBaseRepository<base_Project, Guid> base_Project { set; get; }

        /// <summary>
        ///  
        /// </summary>
        public virtual IBaseRepository<Mapping_AdminProject, Guid> Mapping_AdminProject { set; get; }

        public virtual IBaseRepository<base_Server,Guid> base_Server { set; get; }
    }
}

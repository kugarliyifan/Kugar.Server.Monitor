using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace Kugar.Server.MonitorServer.Data.DbEntities
{
    [Table]
    public  class base_Project:EntityBase
    {
        [Column(IsPrimary = true,InsertValueSql = "NEWID()")]
        public Guid ProjectId { set; get; }

        public string Title { set; get; }
    }
}

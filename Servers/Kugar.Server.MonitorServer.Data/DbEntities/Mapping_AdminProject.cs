using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace Kugar.Server.MonitorServer.Data.DbEntities
{
    [Table]
    public class Mapping_AdminProject:EntityBase
    {
        [Column(IsPrimary = true)]
        public Guid AdminId { set; get; }

        [Column(IsPrimary = true)]
        public Guid ProjectId { set; get; }
    }
}

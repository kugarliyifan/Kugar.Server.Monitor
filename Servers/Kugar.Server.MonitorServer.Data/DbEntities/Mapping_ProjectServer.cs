using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace Kugar.Server.MonitorServer.Data.DbEntities
{
    [Table]
    public class Mapping_ProjectServer:EntityBase
    {
        [Column(IsPrimary = true)]
        public Guid ProjectId { get; set; }

        [Column(IsPrimary = true)]
        public Guid ServerId { set; get; }



    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Server.MonitorServer.Data.DbEntities
{
    public abstract class EntityBase
    {
        public bool IsDeleted { get; set; }
    }
}

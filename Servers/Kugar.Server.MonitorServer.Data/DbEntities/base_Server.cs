using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Server.MonitorServer.Data.DbEntities
{
    public class base_Server
    {
        public Guid ServerId { set; get; }

        public string IpAddress { set; get; }
    }
}

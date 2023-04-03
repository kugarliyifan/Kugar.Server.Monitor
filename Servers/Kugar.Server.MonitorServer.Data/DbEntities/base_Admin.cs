using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace Kugar.Server.MonitorServer.Data.DbEntities
{
    [Table]
    public class base_Admin:EntityBase
    {
        public Guid AdminId { set; get; }

        public string LoginName { set; get; }

        public string Password { set; get; }

        public int State { set; get; }
         
    }
}

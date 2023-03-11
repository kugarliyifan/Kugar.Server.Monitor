using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Server.MonitorCollectors.Core;

namespace Kugar.Server.MonitorCollectorRunner.Submitters
{
    public class HttpSubmitter:IDataSubmitter
    {
        public Task Submit(IEnumerable<EventDataBase> dataList)
        {
            throw new NotImplementedException();
        }
    }
}

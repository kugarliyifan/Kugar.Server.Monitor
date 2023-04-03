using Kugar.Server.MonitorCollectors.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Server.MonitorCollectorRunner.Submitters
{
    public class WebSocketSubmitter: IDataSubmitter
    {
        public Task Submit(IEnumerable<IEventDataBase> dataList)
        {
            throw new NotImplementedException();
        }

        public Task Init()
        {
            throw new NotImplementedException();
        }
    }
}

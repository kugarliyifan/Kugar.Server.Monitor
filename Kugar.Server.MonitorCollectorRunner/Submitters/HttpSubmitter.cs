using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using Kugar.Server.MonitorCollectors.Core;

namespace Kugar.Server.MonitorCollectorRunner.Submitters
{
    public class HttpSubmitter:IDataSubmitter
    {
        public Task Submit(IEnumerable<IEventDataBase> dataList)
        {
            foreach (var item in dataList)
            {
                Console.WriteLine(item.Serialize().ToStringEx());
            }

            return Task.CompletedTask;
        }
    }
}

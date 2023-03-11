using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json;

namespace Kugar.Server.MonitorCollectors.Core
{
    public interface IDataSubmitter
    {
        Task Submit(IEnumerable<EventDataBase> dataList);
    }

    public class DataSubmitter:IDataSubmitter
    {
        public async Task Submit(IEnumerable<EventDataBase> dataList)
        {
            foreach (var item in dataList)
            {
                Console.WriteLine(item.Serialize().ToStringEx(Formatting.None));
            }
             
        }
    }
}

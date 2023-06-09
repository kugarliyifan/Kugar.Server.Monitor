﻿using System;
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
        Task Submit(IEnumerable<IEventDataBase> dataList);

        Task Init();
    }

    public class DataSubmitter:IDataSubmitter
    {
        public async Task Submit(IEnumerable<IEventDataBase> dataList)
        {
            foreach (var item in dataList)
            {
                var data = item.Serialize().ToStringEx(Formatting.None);

                Console.WriteLine(data);
            }
             
        }

        public Task Init()
        {
            throw new NotImplementedException();
        }
    }
}

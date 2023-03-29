using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using Kugar.Core.Services;
using Kugar.Server.MonitorCollectorRunner.Submitters;
using Kugar.Server.MonitorCollectors.Core;

namespace Kugar.Server.MonitorCollectorRunner.Tasks
{
    public class InitHttpSubmitterTask:BackgroundServiceEx
    {
        public InitHttpSubmitterTask(IServiceProvider provider) : base(provider)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var submitter = Provider.GetService<IDataSubmitter>();

            await submitter.Init();
        }
    }
}

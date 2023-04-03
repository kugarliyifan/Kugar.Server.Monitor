using System.Diagnostics;
using Kugar.Server.MonitorCollectors.Core;
using Kugar.Server.MonitorCollectors.Core.Attributes;
using Microsoft.Web.Administration;
using System.Security.Policy;
using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;

namespace Kugar.Server.Monitor.Collectors.IIS
{
    [ExportMonitor]
    [WindowOnly]
    public class IISCollector:UniformSubmitMonitorBase
    {
        public IISCollector(IServiceProvider provider) : base(provider)
        {
            this.Enabled = CustomConfigManager.Default.GetValue("IIS:Enabled", false);
            this.Internal= CustomConfigManager.Default.GetValue("IIS:Internal", 60) * 1000;
        }

        private readonly Dictionary<int, (DateTime lastDt,TimeSpan cpuusage)> _cacheProcessUsages = new();

        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken stoppingToken)
        {
            var server = new ServerManager();
            var sites = server.Sites.Select(site =>
            {
                var application = site.Applications.FirstOrDefault(x => x.Path == "/");
                var applicationPoolName = application?.ApplicationPoolName;
                var processId = server.WorkerProcesses.FirstOrDefault(x => x.AppPoolName == applicationPoolName)
                    ?.ProcessId;

                var item = new IISSiteEventData()
                {
                    EventDt = DateTime.Now,
                    ApplicationPoolName = applicationPoolName??"",
                    ProcessId = processId??-1,
                    SiteName = site.Name
                };

                if (processId>0)
                {
                    var proc = Process.GetProcessById(processId.Value);

                    if (proc!=null)
                    {
                        item.UserName = proc.StartInfo.UserName;
                        item.MemoryUsage = Math.Round((decimal)proc.WorkingSet64 / 1024 / 1024, 3);

                        if (_cacheProcessUsages.TryGetValue(processId.Value,out var oldData))
                        {
                            var cpuUsedMs = (proc.TotalProcessorTime - oldData.cpuusage).TotalMilliseconds;
                            var totalMsPassed = (item.EventDt - oldData.lastDt).TotalMilliseconds;
                            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

                            var cpuUsagePercentage = cpuUsageTotal * 100;

                            item.CpuUsage = (decimal)cpuUsagePercentage;
                        }

                        _cacheProcessUsages.AddOrUpdate(processId.Value, (item.EventDt, proc.TotalProcessorTime));
                    }
                }

                return item;
            });

            await Submit(sites);

        }

        protected override int Internal { get; }

        public override string TypeId { get; set; } = "IISSiteEventData";
    }

    /// <summary>
    /// IIS单个站点的信息
    /// </summary>
    public class IISSiteEventData : IEventDataBase
    {
        public string TypeId => "IISSiteEventData";

        public string SiteName { set; get; }

        public int? ProcessId { set; get; }

        public string ApplicationPoolName { set; get; }

        public decimal MemoryUsage { set; get; }

        public decimal CpuUsage { set; get; }

        public string UserName { set; get; }

        public DateTime EventDt { get; set; }
    }
}
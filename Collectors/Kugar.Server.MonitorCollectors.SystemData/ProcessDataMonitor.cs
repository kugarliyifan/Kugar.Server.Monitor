using System.Collections.Concurrent;
using System.Diagnostics;
using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;
using Kugar.Server.MonitorCollectors.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.Server.MonitorCollectors.SystemData
{
    [ExportMonitor]
    public class ProcessDataMonitor:UniformSubmitMonitorBase
    {
        private HashSet<string> _processIds = null;

        public ProcessDataMonitor(IServiceProvider provider) : base(provider)
        {
            this.Enabled = CustomConfigManager.Default.GetValue<bool>("SystemData:Process:Enabled",true);
            Internal=CustomConfigManager.Default.GetValue<int>("SystemData:Process:Internal",60) * 1000;

            var processIds = CustomConfigManager.Default.GetValue<List<string>>("SystemData:Process:ProcessIds");

            if (processIds.HasData())
            {
                _processIds=new HashSet<string>(processIds,StringComparer.CurrentCultureIgnoreCase);
            }
            else
            {
                _processIds = new HashSet<string>(new[]
                {
                    "w3wp",
                    "sqlservr",
                    "nginx"
                },StringComparer.CurrentCultureIgnoreCase);
            }
        }

        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken stoppingToken)
        {
            try
            {
                await collectProcessData();
                 
            }
            catch (Exception e)
            {
                Console.WriteLine(e); 
            }
        }

        protected override int Internal { get; } = 1000;

        private Dictionary<int, ProcessDataEventData> _cacheProcessLastData =
            new Dictionary<int, ProcessDataEventData>();

        private async Task collectProcessData()
        {
            //var prcesslist = Process.GetProcesses();

            var data=Process.GetProcesses()
                .Where(x =>_processIds.Contains(x.ProcessName))
                .Select(x =>
                { 
                    //using var pcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    return new ProcessDataEventData()
                    {
                        MemoryUsage = Math.Round((decimal)x.WorkingSet64 /1024/1024,3) ,
                        ProcessName = x.ProcessName,
                        ProcessId = x.Id,
                        EventDt = DateTime.Now,
                        CPUTotal = x.TotalProcessorTime,// (decimal?)pcCpuLoad?.NextValue()??0m,
                        UserName = x.StartInfo?.UserName,
                        Arguments = x.StartInfo?.Arguments
                    };
                });

            data.Where(x =>
            {
                var oldData = _cacheProcessLastData.TryGetValue(x.ProcessId, null);

                if (oldData!=null)
                {
                    var cpuUsedMs = (x.CPUTotal - oldData.CPUTotal).TotalMilliseconds;
                    var totalMsPassed = (x.EventDt - oldData.EventDt).TotalMilliseconds;
                    var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

                    var cpuUsagePercentage = cpuUsageTotal * 100;

                    x.CpuUsage = (decimal)cpuUsagePercentage;
                }

                return oldData == null ||
                       oldData.CpuUsage != x.CpuUsage ||
                       oldData.MemoryUsage != x.MemoryUsage;
            });

            await Submit(data);

            foreach (var item in data)
            {
                _cacheProcessLastData.AddOrUpdate(item.ProcessId, item);
            }
            
        }
        

        public override string TypeId { get; set; }
    }



    public class ProcessDataEventData : IEventDataBase
    {
        public string ProcessName { get; set; }

        public int ProcessId { get; set; }

        public decimal MemoryUsage { set; get; }

        public decimal CpuUsage { set; get; }

        public string UserName { set; get; }

        public string TypeId { get; } = "ProcessData";

        public DateTime EventDt { get; set; }

        public string Arguments { set; get; }

        [JsonIgnore]
        public TimeSpan CPUTotal { set; get; }
        
    }
}
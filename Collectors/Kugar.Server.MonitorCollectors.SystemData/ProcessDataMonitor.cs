using System.Collections.Concurrent;
using System.Diagnostics;
using System.Management;
using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;
using Kugar.Server.MonitorCollectors.Core;
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
                    using var pcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    return new ProcessDataEventData()
                    {
                        MemoryUsage = Math.Round((decimal)x.WorkingSet64 /1024/1024,3) ,
                        ProcessName = x.ProcessName,
                        ProcessId = x.Id,
                        EventDt = DateTime.Now,
                        CpuUsage = (decimal?)pcCpuLoad?.NextValue()??0m,
                        UserName = getProcessUserName(x.Id)
                    };
                });

            data.Where(x =>
            {
                var oldData = _cacheProcessLastData.TryGetValue(x.ProcessId, null);

                return oldData == null ||
                       oldData.CpuUsage != x.CpuUsage ||
                       oldData.MemoryUsage != x.MemoryUsage ||
                       oldData.MemoryUsage != x.MemoryUsage;
            });

            await Submit(data);

            foreach (var item in data)
            {
                _cacheProcessLastData.AddOrUpdate(item.ProcessId, item);
            }
            
        }
        
        private string getProcessUserName(int pID)
        {
            string userName = string.Empty;
 
            try
            {
                foreach (ManagementObject item in new ManagementObjectSearcher("Select * from Win32_Process WHERE processID=" + pID).Get())
                {
                    ManagementBaseObject inPar = null;
                    ManagementBaseObject outPar = null;
 
                    inPar = item.GetMethodParameters("GetOwner");
                    outPar = item.InvokeMethod("GetOwner", inPar, null);
 
                    userName = Convert.ToString(outPar["User"]);
 
                    break;
                }
            }
            catch
            {
                userName = "SYSTEM";
            }
 
            return userName;
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
        
    }
}
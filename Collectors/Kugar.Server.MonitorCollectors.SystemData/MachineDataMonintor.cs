using Kugar.Server.MonitorCollectors.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json.Linq;

namespace Kugar.Server.MonitorCollectors.SystemData
{
    [ExportMonitor]
    public class MachineDataMonitor : UniformSubmitMonitorBase
    {
        private MachineDataEventData _cacheLastData = null;

        public MachineDataMonitor(IServiceProvider provider) : base(provider)
        {
            Enabled = CustomConfigManager.Default["SystemData:Machine:Enabled"].ToBool(true);
            Internal=CustomConfigManager.Default["SystemData:Machine:Internal"].ToInt(60) * 1000;
        }

        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken stoppingToken)
        {
            try
            {
                var cpuUsage = collectorCpuData();
                var (totalPhys, freePhys, usagePhys) = collectMemoryData();
               

                var data = new MachineDataEventData()
                {
                    CpuUsageRate = cpuUsage,
                    EventDt = DateTime.Now,
                    TotalPhys = totalPhys,
                    FreePhys = freePhys,
                    UsagePhys = usagePhys 
                };

                if (_cacheLastData == null ||
                    _cacheLastData.TotalPhys != data.TotalPhys ||
                    _cacheLastData.FreePhys != data.FreePhys ||
                    _cacheLastData.UsagePhys != data.UsagePhys ||
                    _cacheLastData.CpuUsageRate != data.CpuUsageRate
                   )
                {
                    await Submit(new[] { data });

                    _cacheLastData = data;
                }
                 

                //await Task.Delay(60 * 1000, stoppingToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e); 
            }
        }

        protected override int Internal { get; } = 60 * 1000;

        private (decimal totalPhys, decimal freePhys, decimal usagePhys) collectMemoryData()
        {
            var mi = GetMemoryStatus();

            var totalPhys = FormatSize(mi.ullTotalPhys);
            var freePhys = FormatSize(mi.ullAvailPhys);
            var usagePhys = FormatSize(totalPhys - freePhys);

            return (totalPhys, freePhys, usagePhys);

            //Console.WriteLine("总内存：" + FormatSize(totalPhys ));
            //Console.WriteLine("已使用：" + FormatSize(freePhys ));
            //Console.WriteLine("可使用：" + FormatSize(totalPhys-freePhys));

        }

        public decimal collectorCpuData()
        { 
            var counters = new PerformanceCounter[System.Environment.ProcessorCount];
            for (int i = 0; i < counters.Length; i++)
            {
                counters[i] = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
                //counters[i].NextValue(); // 这里是为了获得CPU占用率的值
            }  

            return (decimal)counters.Select(x=>x.NextValue()).Average();

            //Console.WriteLine("电脑CPU使用率：" + cpuCounter.NextValue() + "%");
            //Console.WriteLine("电脑可使用内存：" + ramCounter.NextValue() + "MB");
            //Console.WriteLine();

        }


        

        #region Win32Api调用
        private static MEMORY_INFO GetMemoryStatus()
        {
            MEMORY_INFO mi = new MEMORY_INFO();
            mi.dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(mi);
            GlobalMemoryStatusEx(ref mi);
            return mi;
        }

        #region 格式化容量大小
        /// <summary>
        /// 格式化容量大小
        /// </summary>
        /// <param name="size">容量（B）</param>
        /// <returns>返回MB</returns>
        private static decimal FormatSize(decimal size)
        {
            decimal d = (decimal)size;
            int i = 2;
            while ((d > 1024) && (i < 5))
            {
                d /= 1024;
                i++;
            }
            //string[] unit = { "B", "KB", "MB", "GB", "TB" };
            return Math.Round(d, 2);
        }
        #endregion

        public override string TypeId { get; set; }


        #region 获得内存信息API
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalMemoryStatusEx(ref MEMORY_INFO mi);

        //定义内存的信息结构
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_INFO
        {
            public uint dwLength; //当前结构体大小
            public uint dwMemoryLoad; //当前内存使用率
            public ulong ullTotalPhys; //总计物理内存大小
            public ulong ullAvailPhys; //可用物理内存大小
            public ulong ullTotalPageFile; //总计交换文件大小
            public ulong ullAvailPageFile; //总计交换文件大小
            public ulong ullTotalVirtual; //总计虚拟内存大小
            public ulong ullAvailVirtual; //可用虚拟内存大小
            public ulong ullAvailExtendedVirtual; //保留 这个值始终为0
        }
        #endregion
        #endregion
         
    }

    public class MachineDataEventData : EventDataBase
    {
        public override string TypeId { get; } = "MachineData";

        public decimal TotalPhys { set; get; }

        public decimal FreePhys { set; get; }

        public decimal UsagePhys { set; get; }

        public decimal CpuUsageRate { set; get; }
         
        public override JObject Serialize()
        {
            return JObject.FromObject(this);
        }

        public override void LoadFrom(string json)
        {
            throw new NotImplementedException();
        }
    }
}

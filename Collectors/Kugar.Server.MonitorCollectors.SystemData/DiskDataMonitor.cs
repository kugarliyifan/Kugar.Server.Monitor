using Kugar.Server.MonitorCollectors.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json.Linq;
using Kugar.Core.Configuration;

namespace Kugar.Server.MonitorCollectors.SystemData
{
    [ExportMonitor]
    public class DiskDataMonitor : UniformSubmitMonitorBase
    {
        public DiskDataMonitor(IServiceProvider provider) : base(provider)
        {
            Enabled = CustomConfigManager.Default["SystemData:Disk:Enabled"].ToBool(true);
            Internal=CustomConfigManager.Default["SystemData:Disk:Internal"].ToInt(60) * 1000;
        }

        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken stoppingToken)
        {
            try
            {
                var disks = getDiskListInfo();

                await this.Submit(disks);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        protected override int Internal { get; } = 5*60*1000;

        public override string TypeId { get; set; } = "DiskData";

        /// <summary>
        /// 获取硬盘上所有的盘符空间信息列表
        /// </summary>
        /// <returns></returns>
        private IEnumerable<FolderEventData> getDiskListInfo()
        {
            var selectQuery = new SelectQuery("select * from win32_logicaldisk");

            var searcher = new ManagementObjectSearcher(selectQuery);

            var diskcollection = searcher.Get();
            if (diskcollection.HasData())
            {
                foreach (ManagementObject disk in diskcollection)
                {
                    int nType = Convert.ToInt32(disk["DriveType"]);
                    if (nType != Convert.ToInt32(DriveType.Fixed))
                    {
                        continue;
                    }
                    else
                    {
                        yield return new FolderEventData()
                        {
                            EventDt = DateTime.Now,
                            FreeSpace = Convert.ToDouble(disk["FreeSpace"]) / (1024 * 1024 * 1024),
                            SumSpace = Convert.ToDouble(disk["Size"]) / (1024 * 1024 * 1024),
                            Path = disk["DeviceID"].ToString()
                        };

                        //yield return (
                        //    name: disk["DeviceID"].ToString(),
                        //    sumSpace: Convert.ToDouble(disk["Size"]) / (1024 * 1024 * 1024),
                        //    freeSpace: Convert.ToDouble(disk["FreeSpace"]) / (1024 * 1024 * 1024)
                        //);
                    }
                }
            }

        }

        public class FolderEventData : EventDataBase
        {
            public string Path { set; get; }

            public double SumSpace { set; get; }

            public double FreeSpace { set; get; }

            public override string TypeId => "DiskData";

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

}

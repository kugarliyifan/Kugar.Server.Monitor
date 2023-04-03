using Kugar.Server.MonitorCollectors.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json.Linq;
using Kugar.Core.Configuration;
using System.IO;

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
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (var driveInfo in allDrives)
            {

                if (driveInfo.DriveType!= DriveType.Fixed)
                {
                    continue;
                }

                yield return new FolderEventData()
                {
                    EventDt = DateTime.Now,
                    FreeSpace =Math.Round(Convert.ToDouble(driveInfo.TotalFreeSpace) / (1024 * 1024 * 1024),2) ,
                    TotalSpace = Math.Round(Convert.ToDouble(driveInfo.TotalSize) / (1024 * 1024 * 1024),2),
                    Path = driveInfo.Name.ToString().Replace("\\","").Replace(":",""),
                };
                
            }
        } 

        public class FolderEventData : IEventDataBase
        {
            public string Path { set; get; }

            public double TotalSpace { set; get; }

            public double FreeSpace { set; get; }

            public string TypeId => "DiskData";
            public DateTime EventDt { get; set; }

            public JObject Serialize()
            {
                return JObject.FromObject(this);
            }

            public void LoadFrom(string json)
            {
                throw new NotImplementedException();
            }
        }
    }

}

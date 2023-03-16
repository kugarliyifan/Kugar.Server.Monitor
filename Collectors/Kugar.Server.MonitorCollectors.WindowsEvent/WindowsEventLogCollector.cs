using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;
using Kugar.Core.Services;
using Kugar.Server.MonitorCollectors.Core;
using Kugar.Server.MonitorCollectors.Core.ExtMethods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kugar.Server.MonitorCollectors.WindowsEvent
{
    [ExportMonitor]
    public class WindowsEventLogCollector:UniformSubmitMonitorBase
    {
        private List<EventLogWatcher> _watchers = new();
 

        protected override Task Run(IServiceProvider serviceProvider, CancellationToken stoppingToken)
        { 
            return Task.CompletedTask;
        }
 

        protected override int Internal { get; } = 10_000_000;

        public override void Dispose()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }
            base.Dispose();
        }

        private void Ew_EventRecordWritten(object? sender, EventRecordWrittenEventArgs e)
        {
            //Console.WriteLine(JsonConvert.SerializeObject(e.EventRecord));
            //Console.WriteLine(JsonConvert.SerializeObject(e.EventException));

            var data = new WindowsEventLogEventData()
            {
                EventDt = e.EventRecord?.TimeCreated ?? DateTime.Now,
                EventRecordId = e.EventRecord?.Id??-1,
                Description = e.EventRecord?.FormatDescription()??"",
                Exception = JsonConvert.SerializeObject(e.EventException),
                LogName = e.EventRecord?.LogName,
                Level = e.EventRecord.Level??0,
                ProviderName = e.EventRecord.ProviderName,
                //Message = e.EventRecord
                //Properies = e.EventRecord?.Properties!=null?(new JArray(e.EventRecord.Properties)).ToStringEx():String.Empty,
                RecordId = e.EventRecord?.RecordId,
                ActivityId = e.EventRecord?.ActivityId??Guid.Empty,
            };

             this.Submit(new []{data});
        }


        public override string TypeId { get; set; } = "WindowsEventLog";

        public WindowsEventLogCollector(IServiceProvider provider) : base(provider)
        {
            var logNames = CustomConfigManager.Default["WindowsEventLogs:Names"].ToStringEx().Split(',');

            foreach (var logName in logNames)
            {
                EventLogWatcher ew = new (new EventLogQuery(logName, PathType.LogName));

                ew.EventRecordWritten += Ew_EventRecordWritten;

                ew.Enabled = true;

                _watchers.Add(ew);
            }
        }
    }

    public class WindowsEventLogEventData : IEventDataBase
    {
        public string TypeId => "WindowsEventLog";
        public DateTime EventDt { get; set; }

        public long? RecordId { set; get; }

        public string LogName { set; get; }

        public string ProviderName { set; get; }

        public int EventRecordId { set; get; }

        public Guid ActivityId { set; get; }

        public int Level { set; get; }
        

        public string Description { set; get; }

        public string Exception { set; get; }
        
        public void LoadFrom(string json)
        {
            var j = JObject.Parse(json);

            this.RecordId = j.GetLong("RecordId", 0);
            this.LogName = j.GetString("LogILogNamed");
            this.EventRecordId = j.GetInt("EventRecordId");
            this.EventRecordId = j.GetInt("EventRecordId");
            this.ActivityId = j.GetGuid("ActivityId");
            this.Description = j.GetString("Description");
            this.Exception = j.GetString("Exception");
            this.Level = j.GetInt("Exception");
            this.ProviderName = j.GetString("ProviderName");
        }
    }

    public class WindowsEventLogTest : TimerHostedService
    { 
        public WindowsEventLogTest(IServiceProvider provider) : base(provider)
        {
            this.Enabled = true;
        }

        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken stoppingToken)
        {
            EventLog eventLog = new EventLog();
            eventLog.Source = "Application";
            //eventLog.
            eventLog.WriteEntry("This is a test message.", EventLogEntryType.Information);
 
            //eventLog.Source = "MyEventLogTarget";
            //eventLog.WriteEntry("This is a test message.", EventLogEntryType.Information);
            
        }

        protected override int Internal => 5000;
    }
}
using Newtonsoft.Json.Linq;

namespace Kugar.Server.MonitorCollectors.Core
{
    public abstract class EventDataBase
    {
        public abstract string TypeId { get; }

        public DateTime EventDt { set; get; }

        public abstract JObject Serialize();

        public abstract void LoadFrom(string json);
    }
}
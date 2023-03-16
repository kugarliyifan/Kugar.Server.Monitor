using Newtonsoft.Json.Linq;

namespace Kugar.Server.MonitorCollectors.Core
{
    public interface IEventDataBase
    {
        string TypeId { get; }

        public DateTime EventDt { set; get; }

        public JObject Serialize()
        {
            return JObject.FromObject(this);
        }

        public void LoadFrom(string json){}
    }
}
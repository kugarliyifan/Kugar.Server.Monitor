using FreeRedis;
using Kugar.Server.MonitorCollectors.Core;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Text.RegularExpressions;
using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Kugar.Server.MonitorCollectors.Redis
{
    public class RedisMonitor : UniformSubmitMonitorBase
    {
        private List<RedisClient> _connClients = new();
        private int _internal = 0;

        public RedisMonitor(IServiceProvider provider) : base(provider)
        {
            //var configuration = (IConfiguration)provider.GetService(typeof(IConfiguration));

            _internal = CustomConfigManager.Default.GetValue<int>("Redis:Internal");// ["Redis:Internal"].Get session["Internal"].ToInt(20) * 1000;

            var connStrList = CustomConfigManager.Default.GetArray<string>("Redis:ConnStr");
             
            foreach (var item in connStrList)
            {
                try
                {
                    var db = new RedisClient(ConnectionStringBuilder.Parse(item));
                    
                    _connClients.Add(db);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }

            }
        }
 
        private static Regex _dbRegex= new Regex("^db[1-9]\\d*|0$", RegexOptions.Compiled);

        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken stoppingToken)
        {
            var list = new List<RedisEventData>();

            foreach (var client in _connClients)
            {
                var args = client.Info();

                using var lst = new StringReader(args); 
                var json = new JObject();

                var line = lst.ReadLine(); 

                while (line!=null)
                {
                    if (line.Length>0)
                    {
                        if (line[0] == '#')
                        {
                            //section
                        } 
                        else 
                        {
                            var item = line.Split(':');
                            var key = item[0].Trim();
                            var value = item[1].Trim();

                            if (_dbRegex.IsMatch(key))
                            {
                                json.Add(key,value);
                            }
                            else
                            {
                                json.Add(key, value.ToJsonToken());
                            } 
                        } 
                    }
                     
                    line = lst.ReadLine();

                }

                list.Add(new RedisEventData(json));
                 
            }

            await this.Submit(list);
        }

        protected override int Internal => _internal;

        public override string TypeId { get; set; }
    }

    public class RedisEventData :DynamicObject, IEventDataBase
    {
        public JObject _json = null;
         
        public RedisEventData(JObject json)
        {
            _json = json;
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            if (!_json.ContainsKey(binder.Name))
            {
                return false;
            } 

            _json[binder.Name]= JToken.FromObject(value);

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = null;

            if (!_json.ContainsKey(binder.Name))
            {
                return false;
            }
            result= _json[binder.Name].ToPrimitiveValue();

            //return base.TryGetMember(binder, out result);

            return true;
        }
         
        public string TypeId => "RedisData";


        public DateTime EventDt { get; set; }
        public JObject Serialize()
        {
            _json.Add("TypeId",TypeId);

            return _json;
        }

        public void LoadFrom(string json)
        {
            throw new NotImplementedException();
        }
    }
}
using Kugar.Core.Serialization;
using Kugar.Server.MonitorCollectors.Core;
using Kugar.Server.MonitorCollectors.Redis;
using Kugar.Server.MonitorCollectors.SQLServer;
using Kugar.Server.MonitorCollectors.SystemData;
using Kugar.Server.MonitorCollectors.WindowsEvent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Kugar.MonitorCollector.Tester
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddJsonFile("appsettings.json",false,true);
                }).
                ConfigureServices((hostContext, services) =>
                {
                    JsonConvert.DefaultSettings = () =>
                    {
                        return new JsonSerializerSettings()
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            Converters = new JsonConverter[]
                            {
                                new GuidJsonConverter(),  
                            }
                        };
                    };
                      
                    //类似startUp
                    services.AddHostedService<ProcessDataMonitor>();
                    //services.AddHostedService<RedisMonitor>();
                   // services.AddHostedService<MachineDataMonitor>();
                    //services.AddHostedService<WindowsEventLogCollector>();
                    //services.AddHostedService<WindowsEventLogTest>();


                }).UseConsoleLifetime();

             await builder.Build().RunAsync(); 
        }
    } 
}
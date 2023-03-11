using Kugar.Server.MonitorCollectors.Core;
using Kugar.Server.MonitorCollectors.SystemData;
using Kugar.Server.MonitorCollectors.WindowsEvent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kugar.MonitorCollector.Tester
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddJsonFile("appsettings.json");
                }).
                ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<DataSubmitter>();

                    //类似startUp
                    services.AddHostedService<ProcessDataMonitor>();
                    //services.AddHostedService<MachineDataMonitor>();
                    //services.AddHostedService<WindowsEventLogCollector>();
                    //services.AddHostedService<WindowsEventLogTest>();


                }).UseConsoleLifetime();

             await builder.Build().RunAsync(); 
        }
    }
}
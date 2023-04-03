using System.Runtime.InteropServices;
using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;
using Kugar.Server.MonitorCollectorRunner.Submitters;
using Kugar.Server.MonitorCollectorRunner.Tasks;
using Kugar.Server.MonitorCollectors.Core;
using Kugar.Server.MonitorCollectors.Core.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Kugar.Server.MonitorCollectorRunner
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

                    //类似startUp
                    //services.AddHostedService<ProcessDataMonitor>();
                    //services.AddHostedService<MachineDataMonitor>();
                    //services.AddHostedService<WindowsEventLogCollector>();
                    //services.AddHostedService<WindowsEventLogTest>();
                    var plugins =CustomConfigManager.Default.GetArray<string>("plugins");

                    services.AddSingleton<IDataSubmitter, HttpSubmitter>();

                    services.AddHostedService<InitHttpSubmitterTask>();

                    foreach (var plugin in plugins)
                    {
                        var folder = Path.GetDirectoryName(plugin);
                        var loader = new AssemblyLoader(folder);
                        var types=loader.Load(plugin);

                        var monitors = types.Where(x =>!x.IsAbstract && x.IsPublic && x.GetCustomAttributes(typeof(ExportMonitorAttribute),true).HasData());

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            monitors = monitors.Where(x =>
                            {
                                var attrs = x.GetAttribute<LinuxOnlyAttribute>();

                                return attrs == null;
                            });
                        }
                        else
                        {
                            monitors= monitors.Where(x =>
                            {
                                var attrs = x.GetAttribute<WindowOnlyAttribute>();

                                return attrs == null;
                            });
                        }

                        foreach (var monitor in monitors)
                        { 
                            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHostedService), monitor));
                        }

                    }


                }).UseConsoleLifetime();

            await builder.Build().RunAsync(); 
        }
    }
}
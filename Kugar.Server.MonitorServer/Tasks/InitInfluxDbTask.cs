using Kugar.Core.Services;

namespace Kugar.Server.MonitorServer.Tasks
{
    public class InitInfluxDbTask:BackgroundServiceEx
    {
        public InitInfluxDbTask(IServiceProvider provider) : base(provider)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
        }
    }
}

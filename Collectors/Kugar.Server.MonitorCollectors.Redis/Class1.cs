using Kugar.Server.MonitorCollectors.Core;

namespace Kugar.Server.MonitorCollectors.Redis
{
    public class RedisMonitor : UniformSubmitMonitorBase
    {
        public RedisMonitor(IServiceProvider provider) : base(provider)
        {
        }

        protected override Task Run(IServiceProvider serviceProvider, CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        protected override int Internal => "RedisData";

        public override string TypeId { get; set; }
    }
}
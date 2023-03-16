using Kugar.Core.Configuration;
using Kugar.Core.ExtMethod;
using Kugar.Core.Services;
using Newtonsoft.Json;

namespace Kugar.Server.MonitorCollectors.Core;

public abstract class MonitorBase:TimerHostedService, IDisposable
{
    protected abstract Task Submit(IEnumerable<IEventDataBase> data);
      
     
    public abstract string TypeId { set; get; }

    public virtual void Dispose()
    {
         
    }

    protected MonitorBase(IServiceProvider provider) : base(provider)
    {
    }
}

/// <summary>
/// 由采集器自行提交数据的基类
/// </summary>
public abstract class SelfSubmitMonitorBase:MonitorBase
{
    protected SelfSubmitMonitorBase(IServiceProvider provider) : base(provider)
    {
    }
     
}

/// <summary>
/// 统一由框架提交日志数据的基类
/// </summary>
public abstract class UniformSubmitMonitorBase : MonitorBase
{
    protected override async Task Submit(IEnumerable<IEventDataBase> data)
    {
        var p = (IDataSubmitter)Provider.GetService(typeof(IDataSubmitter));

        await p.Submit(data);
    }



    protected UniformSubmitMonitorBase(IServiceProvider provider) : base(provider)
    {
    }
}


public interface ITimerTask
{
    Task Execute();
     
}

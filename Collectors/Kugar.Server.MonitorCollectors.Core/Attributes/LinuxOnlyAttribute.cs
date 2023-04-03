using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Server.MonitorCollectors.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
    public class LinuxOnlyAttribute:Attribute
    {
    }
}

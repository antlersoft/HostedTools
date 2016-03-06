using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace com.antlersoft.HostedTools.Framework.Interface.Plugin
{
    public interface ICancelableMonitor : IWorkMonitor
    {
        bool IsCanceled { get; set; }
        CancellationToken Cancellation { get; }
    }
}

using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    public interface IHtValueSink : IHostedObject
    {
        void ReceiveRows(IEnumerable<IHtValue> rows, IWorkMonitor monitor);
        string SinkDescription { get; }
    }
}

using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    public interface IHtValueSource : IHostedObject
    {
        IEnumerable<IHtValue> GetRows(IWorkMonitor monitor);
    }
}

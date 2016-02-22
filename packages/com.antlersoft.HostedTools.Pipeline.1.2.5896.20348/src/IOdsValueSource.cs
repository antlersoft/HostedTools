using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    public interface IHtValueSource : IHostedObject
    {
        IEnumerable<IHtValue> GetRows();
        string SourceDescription { get; }
    }
}

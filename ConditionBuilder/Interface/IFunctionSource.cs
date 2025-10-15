using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Interface {
    public interface IFunctionSource : IHostedObject {
        IEnumerable<IFunctionNamespace> AvailableNamespaces { get; }
    }
}
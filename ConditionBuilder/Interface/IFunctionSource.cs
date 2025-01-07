using System.Collections.Generic;

namespace com.antlersoft.HostedTools.ConditionBuilder.Interface {
    public interface IFunctionSource {
        IEnumerable<IFunctionNamespace> AvailableNamespaces { get; }
    }
}
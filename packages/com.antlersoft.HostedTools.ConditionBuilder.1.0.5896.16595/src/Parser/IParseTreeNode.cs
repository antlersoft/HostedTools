using System;

namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public interface IParseTreeNode
    {
        Type ResultType { get; }
        Func<object, object> GetFunctor();
    }
}

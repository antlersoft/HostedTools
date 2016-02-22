
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    public interface INoSqlSource : IPlugin
    {
        INoSql NoSql { get; }
        IConditionFactory ConditionFactory { get; }
        string Description { get; }
    }
}

using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface ISqlCommandTimeout : IHostedObject
    {
        int TimeoutSeconds { get; }
    }
}

using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface IConstraint : IHostedObject
    {
        string Name { get; }
        ITable ReferencedTable { get; }
        IIndexSpec LocalColumns { get; }
        IIndexSpec ReferencedColumns { get; }
    }
}

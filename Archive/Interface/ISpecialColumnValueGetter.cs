using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Sql.Interface;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Archive.Interface
{
    public interface ISpecialColumnValueGetter : IAggregatable
    {
        string Name { get; }
        IEnumerable<Dictionary<string, IHtValue>> GetColumnValueSets(IIndexSpec columns, IHtValue row);
    }
}

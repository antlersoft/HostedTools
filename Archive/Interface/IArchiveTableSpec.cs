using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Sql.Interface;

namespace com.antlersoft.HostedTools.Archive.Interface
{
    public interface IArchiveTableSpec : IHostedObject
    {
        ITable Table { get; }
        IHtExpression TableFilter { get; }
        IList<ITable> ImplicitReferences { get; }
    }
}

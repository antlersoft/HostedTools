using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Sql.Interface;
using System.Collections.Generic;
using System.Linq;

namespace com.antlersoft.HostedTools.Archive.Model
{
    public class ArchiveTableSpec : HostedObjectBase, IArchiveTableSpec
    {
        public ArchiveTableSpec(ITable table, IHtExpression filter, IEnumerable<ITable> implicitReferences=null)
        {
            Table = table;
            TableFilter = filter;
            ImplicitReferences = implicitReferences == null ? new List<ITable>() : implicitReferences.ToList();
        }
        public ITable Table { get; }

        public IHtExpression TableFilter { get; }

        public IList<ITable> ImplicitReferences { get; }
    }
}

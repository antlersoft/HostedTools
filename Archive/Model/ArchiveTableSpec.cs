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
        public ArchiveTableSpec(ITable table, IHtExpression filter, IEnumerable<ITable> implicitReferences=null, string sqlQuery = null)
        {
            Table = table;
            TableFilter = filter;
            ImplicitReferences = implicitReferences == null ? new List<ITable>() : implicitReferences.ToList();
            SqlQuery = sqlQuery;            
        }
        public ITable Table { get; }

        public IHtExpression TableFilter { get; }

        public IList<ITable> ImplicitReferences { get; }

        /// <summary>
        /// If this is set, retrieve data for this table with this query regardless of the repository configuration
        /// </summary>
        public string SqlQuery { get; }
    }
}

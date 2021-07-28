using com.antlersoft.HostedTools.Sql.Interface;
using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Sql.Model
{
    public class RedshiftConnectionSource : PostgreSqlConnectionSource
    {
        public RedshiftConnectionSource(string connectionString, int timeout = 5)
        : base(connectionString, timeout)
        {

        }

        public override string GetDistinctText(IEnumerable<Tuple<string, ITable>> aliasesAndTables)
        {
            return "distinct";
        }

        public override IIndexSpec GetPrimaryKey(IBasicTable table)
        {
            return null;
        }

        public override Dictionary<string, IIndexSpec> GetIndexInfo(IBasicTable table)
        {
            return new Dictionary<string, IIndexSpec>();
        }

        public override IEnumerable<IConstraint> GetReferentialConstraints(IBasicTable table, Func<string, string, ITable> tableGetter)
        {
            return new IConstraint[] { };
        }
    }
}

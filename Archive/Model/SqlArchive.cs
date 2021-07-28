using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Pipeline;
using com.antlersoft.HostedTools.Sql;
using com.antlersoft.HostedTools.Sql.Interface;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace com.antlersoft.HostedTools.Archive.Model
{
    class SqlArchive : HostedObjectBase, IArchive
    {
        List<SqlArchiveTable> _tables;
        ISqlConnectionSource _getConnection;

        internal SqlArchive(IArchiveSpec spec, ISqlConnectionSource getConnection, List<SqlArchiveTable> tables)
        {
            Spec = spec;
            _getConnection = getConnection;
            _tables = tables;
        }
        public IArchiveSpec Spec { get; }

        public IEnumerable<ITable> Tables => _tables.Select(t => t.Table);

        public IEnumerable<IHtValue> GetRows(ITable table)
        {
            var archiveTable = _tables.First(t => t.Table.Schema == table.Schema && t.Table.Name == table.Name);
            int timeoutSeconds = 30;
            if (_getConnection.Cast<ISqlCommandTimeout>() is ISqlCommandTimeout ct)
            {
                timeoutSeconds = ct.TimeoutSeconds;
            }
            return SqlUtil.GetRows(_getConnection, archiveTable.GetQuery(_getConnection.Cast<IDistinctHandling>(), _getConnection.Cast<ISqlColumnInfo>()), timeoutSeconds);
        }
    }
}

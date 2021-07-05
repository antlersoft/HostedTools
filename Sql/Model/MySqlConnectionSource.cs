using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Sql.Interface;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace com.antlersoft.HostedTools.Sql.Model
{
    public class MySqlConnectionSource : HostedObjectBase, ISqlConnectionSource, ISqlPrimaryKeyInfo, ISqlReferentialConstraintInfo
    {
        private string _connString;
        public MySqlConnectionSource(string connString)
        {
            _connString = connString;
        }
        public DbConnection GetConnection()
        {
            return new MySqlConnection(_connString);
        }

        public IIndexSpec GetPrimaryKey(IBasicTable table)
        {
            IndexSpec primaryKey = null;
            foreach (var row in SqlUtil.GetRows(this, $"SELECT column_name, ordinal_position FROM information_schema.key_column_usage where table_name = '{table.Name}' and table_schema = '{table.Schema}' AND constraint_name = 'PRIMARY' ORDER BY ordinal_position", 5).Select(r => SqlUtil.LowerCaseKeys(r)))
            {
                if (primaryKey == null)
                {
                    primaryKey = new IndexSpec();
                }
                primaryKey.AddColumn(new IndexColumn(table[row["column_name"].AsString]));
            }
            return primaryKey;
        }

        public IEnumerable<IConstraint> GetReferentialConstraints(IBasicTable table, Func<string, string, ITable> tableGetter)
        {
            foreach (var crow in SqlUtil.GetRows(this,
            $@"
select rc.constraint_schema, rc.constraint_name, rc.unique_constraint_schema, rc.unique_constraint_name, rc.referenced_table_name
from information_schema.referential_constraints rc
where rc.table_name = '{table.Name}'
and rc.constraint_schema = '{table.Schema}'
            ", 30).Select(r => SqlUtil.LowerCaseKeys(r)))
            {
                ITable referencedTable = tableGetter(crow["unique_constraint_schema"].AsString, crow["referenced_table_name"].AsString);
                IndexSpec localColumns = new IndexSpec();
                IndexSpec referencedColumns = new IndexSpec();
                foreach (var row in SqlUtil.GetRows(this, $@"
select kc.column_name, kc.ordinal_position, kcr.column_name as referenced_column_name
from information_schema.key_column_usage kc, information_schema.key_column_usage kcr
where kc.constraint_schema = '{crow["constraint_schema"].AsString}'
and kc.constraint_name = '{crow["constraint_name"].AsString}'
and kc.table_schema = '{table.Schema}'
and kc.table_name = '{table.Name}'
and kcr.constraint_schema = '{crow["unique_constraint_schema"].AsString}'
and kcr.constraint_name = '{crow["unique_constraint_name"].AsString}'
and kcr.table_schema = '{referencedTable.Schema}'
and kcr.table_name = '{referencedTable.Name}'
and kc.position_in_unique_constraint = kcr.ordinal_position
order by kc.ordinal_position 
                ", 30).Select(r => SqlUtil.LowerCaseKeys(r)))
                {
                    localColumns.AddColumn(new IndexColumn(table[row["column_name"].AsString]));
                    referencedColumns.AddColumn(new IndexColumn(referencedTable[row["referenced_column_name"].AsString]));
                }
                yield return new Constraint(crow["constraint_name"].AsString, referencedTable, localColumns, referencedColumns);
            }
        }

    }
}

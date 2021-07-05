using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Sql.Interface;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace com.antlersoft.HostedTools.Sql.Model
{
    public class PostgreSqlConnectionSource : HostedObjectBase, ISqlConnectionSource, ISqlIndexInfo, ISqlReferentialConstraintInfo
    {
        private string _connString;
        private int _timeout;

        public PostgreSqlConnectionSource(string connString, int timeout=5)
        {
            _connString = connString;
            _timeout = timeout;
        }
        public DbConnection GetConnection()
        {
            return new Npgsql.NpgsqlConnection(_connString);
        }

        public IIndexSpec GetPrimaryKey(IBasicTable table)
        {
            IndexSpec primaryKey = null;
            foreach (var row in SqlUtil.GetRows(this,
$@"select column_name, ordinal_position from information_schema.table_constraints tc
join information_schema.key_column_usage ku on tc.constraint_name = ku.constraint_name and tc.constraint_schema = ku.constraint_schema
where tc.constraint_type = 'PRIMARY KEY'
and tc.table_schema = '{table.Schema}'
and tc.table_name = '{table.Name}'
order by ordinal_position", _timeout))
            {
                if (primaryKey == null)
                {
                    primaryKey = new IndexSpec();
                }
                primaryKey.AddColumn(new IndexColumn(table[row["column_name"].AsString]));
            }
            return primaryKey;
        }

        public virtual Dictionary<string, IIndexSpec> GetIndexInfo(IBasicTable table)
        {
            var result = new Dictionary<string, IIndexSpec>();
            string currentIndexName = null;
            IndexSpec currentIndexColumns = null;
            foreach (var row in SqlUtil.GetRows(this,
$@"select
    t.relname as table_name,
    n.nspname as table_schema,
    i.relname as index_name,
    a.attname as column_name
from
    pg_class t,
    pg_class i,
    pg_index ix,
    pg_namespace n,
    pg_attribute a
where
    t.oid = ix.indrelid
    and i.oid = ix.indexrelid
    and a.attrelid = t.oid
    and a.attnum = ANY(ix.indkey)
    and t.relkind = 'r'
    and t.relname = '{table.Name}'
    and n.nspname = '{table.Schema}'
    and n.oid = t.relnamespace
order by
    t.relname,
    i.relname",
                    _timeout))
            {
                var indexName = row["index_name"].AsString;
                if (indexName != currentIndexName)
                {
                    currentIndexColumns = new IndexSpec();
                    result[indexName] = currentIndexColumns;
                    currentIndexName = indexName;
                }
                currentIndexColumns.AddColumn(new IndexColumn(table[row["column_name"].AsString]));
            }
            return result;
        }

        public IEnumerable<IConstraint> GetReferentialConstraints(IBasicTable table, Func<string, string, ITable> tableGetter)
        {
            foreach (var crow in SqlUtil.GetRows(this,
            $@"
select rc.constraint_schema, rc.constraint_name, tcr.table_schema, tcr.table_name, rc.unique_constraint_name, rc.unique_constraint_schema from information_schema.referential_constraints rc
join information_schema.table_constraints tc on rc.constraint_schema = tc.constraint_schema and rc.constraint_name = tc.constraint_name
join information_schema.table_constraints tcr on rc.unique_constraint_schema = tcr.constraint_schema and rc.unique_constraint_name = tcr.constraint_name
where tc.table_name = '{table.Name}'
and tc.table_schema = '{table.Schema}'
            ", _timeout))
            {
                ITable referencedTable = tableGetter(crow["table_schema"].AsString, crow["table_name"].AsString);
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
                ", _timeout))
                {
                    localColumns.AddColumn(new IndexColumn(table[row["column_name"].AsString]));
                    referencedColumns.AddColumn(new IndexColumn(table[row["referenced_column_name"].AsString]));
                }
                yield return new Constraint(crow["constraint_name"].AsString, referencedTable, localColumns, referencedColumns);
            }
        }
    }
}

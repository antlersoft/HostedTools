using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Sql.Interface;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace com.antlersoft.HostedTools.Sql.Model
{
    public class PostgreSqlConnectionSource : HostedObjectBase, ISqlConnectionSource, ISqlIndexInfo, ISqlReferentialConstraintInfo,
        IDistinctHandling, ISqlColumnInfo, ISqlPrimaryKeyInfo
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
            var result = new Npgsql.NpgsqlConnection(_connString);
            result.Open();
            result.TypeMapper.UseNetTopologySuite();
            return result;
        }

        public IIndexSpec GetPrimaryKey(IBasicTable table)
        {
            IndexSpec primaryKey = null;
            foreach (var row in SqlUtil.GetRows(this,
$@"SELECT               
  pg_attribute.attname, 
  format_type(pg_attribute.atttypid, pg_attribute.atttypmod) 
FROM pg_index, pg_class, pg_attribute, pg_namespace 
WHERE 
  pg_class.oid = '{table.Name}'::regclass AND 
  indrelid = pg_class.oid AND 
  nspname = '{table.Schema}' AND 
  pg_class.relnamespace = pg_namespace.oid AND 
  pg_attribute.attrelid = pg_class.oid AND 
  pg_attribute.attnum = any(pg_index.indkey)
 AND indisprimary", _timeout))
            {
                if (primaryKey == null)
                {
                    primaryKey = new IndexSpec();
                }
                primaryKey.AddColumn(new IndexColumn(table[row["attname"].AsString]));
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
            IndexSpec localColumns = null;
            IndexSpec referencedColumns = null;
            ITable referencedTable = null;
            string currentConstraint = null;
            // Omitted columns c.conntype constraint_type, pg_get_constraint_def(c.oid) as definition
            foreach (var crow in SqlUtil.GetRows(this,
            $@"
WITH unnested_confkey AS (
  SELECT oid, unnest(confkey) as confkey
  FROM pg_constraint
),
unnested_conkey AS (
  SELECT oid, unnest(conkey) as conkey
  FROM pg_constraint
)
select
  c.conname                   AS constraint_name,
  nsp1.nspname                AS constraint_schema,
  tbl.relname                 AS constraint_table,
  col.attname                 AS constraint_column,
  nsp2.nspname                AS referenced_schema,
  referenced_tbl.relname      AS referenced_table,
  referenced_field.attname    AS referenced_column
FROM pg_constraint c
LEFT JOIN unnested_conkey con ON c.oid = con.oid
LEFT JOIN pg_class tbl ON tbl.oid = c.conrelid
LEFT JOIN pg_attribute col ON (col.attrelid = tbl.oid AND col.attnum = con.conkey)
LEFT JOIN pg_class referenced_tbl ON c.confrelid = referenced_tbl.oid
LEFT JOIN unnested_confkey conf ON c.oid = conf.oid
LEFT JOIN pg_attribute referenced_field ON (referenced_field.attrelid = c.confrelid AND referenced_field.attnum = conf.confkey)
JOIN pg_namespace nsp1 ON tbl.relnamespace = nsp1.oid
JOIN pg_namespace nsp2 ON referenced_tbl.relnamespace = nsp2.oid
WHERE c.contype = 'f'
AND nsp1.nspname = '{table.Schema}'
AND tbl.relname = '{table.Name}'
            ", _timeout))
            {
                var constraintName = crow["constraint_name"].AsString;
                if (currentConstraint == null || currentConstraint != constraintName)
                {
                    if (currentConstraint != null)
                    {
                        yield return new Constraint(currentConstraint, referencedTable, localColumns, referencedColumns);
                    }
                    currentConstraint = constraintName;
                    referencedTable = tableGetter(crow["referenced_schema"].AsString, crow["referenced_table"].AsString);
                    localColumns = new IndexSpec();
                    referencedColumns = new IndexSpec();
                }
                localColumns.AddColumn(new IndexColumn(table[crow["constraint_column"].AsString]));
                referencedColumns.AddColumn(new IndexColumn(referencedTable[crow["referenced_column"].AsString]));
            }
            if (currentConstraint != null)
            {
                yield return new Constraint(currentConstraint, referencedTable, localColumns, referencedColumns);
            }
        }

        public string GetDistinctText(IEnumerable<Tuple<string, ITable>> aliasesAndTables)
        {
            var builder = new StringBuilder();

            var first = true;
            builder.Append("distinct on (");

            foreach (var at in aliasesAndTables)
            {
                var pk = at.Item2.PrimaryKey;
                if (pk == null)
                {
                    // Fail path
                    return "distinct";
                }
                foreach (var col in pk.Columns)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.Append(',');
                    }
                    builder.Append(at.Item1);
                    builder.Append('.');
                    builder.Append('"');
                    builder.Append(col.Field.Name);
                    builder.Append('"');
                }
            }
            builder.Append(')');
            return builder.ToString();
        }

        public string GetColumnReference(IField field)
        {
            string result = field.Name;
            if (result.ToLowerInvariant() != result)
            {
                result = $"\"{result}\"";
            }
            return result;
        }

        public StringBuilder AppendColumnLiteral(StringBuilder builder, IField field, IHtValue value)
        {
            if (field.DataType == "geometry" && value!=null && ! value.IsEmpty && value.AsString.Length>0)
            {
                builder.Append("ST_GeomFromText('");
                builder.Append(value.AsString);
                builder.Append("', 4326)");
            }
            else
            {
                SqlUtil.AppendSqlLiteral(builder, value);
            }
            return builder;
        }
    }
}

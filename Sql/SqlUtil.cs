using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using com.antlersoft.HostedTools.Sql.Interface;
using com.antlersoft.HostedTools.Sql.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace com.antlersoft.HostedTools.Sql
{
    public static class SqlUtil
    {
        public static string SqlDateFormat = "yyyy-MM-dd hh:mm:ss";
        public static IHtValue GetColumnValue(IDataReader reader, int col)
        {
            object val;
            if (reader.IsDBNull(col))
            {
                return new JsonHtValue();
            }
            try
            {
                val = reader.GetValue(col);
            }
            catch (FormatException)
            {
                val = null;
            }
            catch (NullReferenceException)
            {
                val = null;
            }
            if (val == null)
            {
                return new JsonHtValue();
            }
            Type t = val.GetType();
            if (t == typeof(int))
            {
                return new JsonHtValue((int)val);
            }
            if (t == typeof(uint))
            {
                return new JsonHtValue((long)(uint)val);
            }
            else if (t == typeof(long))
            {
                return new JsonHtValue((long)val);
            }
            else if (t == typeof(ulong))
            {
                return new JsonHtValue((long)(ulong)val);
            }
            else if (t == typeof(double))
            {
                return new JsonHtValue((double)val);
            }
            else if (t == typeof(float))
            {
                return new JsonHtValue((float)val);
            }
            else if (t == typeof(short))
            {
                return new JsonHtValue((short)val);
            }
            else if (t == typeof(ushort))
            {
                return new JsonHtValue((ushort)val);
            }
            else if (t == typeof(byte))
            {
                return new JsonHtValue((short)(byte)val);
            }
            else if (t == typeof(sbyte))
            {
                return new JsonHtValue((short)(sbyte)val);
            }
            else if (t == typeof(char))
            {
                return new JsonHtValue((short)((char)val));
            }
            else if (t == typeof(bool))
            {
                return new JsonHtValue((bool)val);
            }
            else if (t == typeof(DateTime))
            {
                JsonHtValue dateValue = new JsonHtValue();
                dateValue["Ticks"] = new JsonHtValue(((DateTime)val).Ticks);
                return dateValue;
            }
            else
            {
                return new JsonHtValue(val.ToString());
            }
        }

        public static string InClause(IEnumerable<string> strings)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            bool first = true;
            foreach (var i in strings)
            {
                if (!first)
                {
                    sb.Append(',');
                }
                else
                {
                    first = false;
                }
                sb.Append('\'');
                foreach (var ch in i)
                {
                    if (ch == '\'')
                    {
                        sb.Append('\'');
                    }
                    sb.Append(ch);
                }
                sb.Append('\'');
            }
            sb.Append(')');
            return sb.ToString();
        }

        public static string InClause(IEnumerable<IHtValue> values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            bool first = true;
            foreach (var i in values)
            {
                if (!first)
                {
                    sb.Append(',');
                }
                else
                {
                    first = false;
                }
                AppendSqlLiteral(sb, i);
            }
            sb.Append(')');
            return sb.ToString();
        }

        public static void InsertRows(ISqlConnectionSource connectionSource, IBasicTable table, IEnumerable<IHtValue> rows, int commandTimeout=30, int maxLength=30000)
        {
            StringBuilder currentStatement = new StringBuilder();
            ISqlColumnInfo columnInfo = connectionSource.Cast<ISqlColumnInfo>() ?? new SimpleColumnInfo();

            using (var conn = connectionSource.GetConnection())
            {
                foreach (var row in rows)
                {
                    var nextRow = SingleInsertStatement(table, columnInfo, row);
                    if (connectionSource.Cast<IWorkMonitorSource>() is IWorkMonitorSource wms)
                    {
                        wms.GetMonitor().Writer.WriteLine(nextRow);
                    }
                    if (nextRow.Length + currentStatement.Length > maxLength)
                    {
                        if (currentStatement.Length == 0)
                        {
                            currentStatement.Append(nextRow);
                        }
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = currentStatement.ToString();
                        cmd.ExecuteNonQuery();
                        currentStatement.Clear();
                    }
                    else
                    {
                        currentStatement.Append(nextRow);
                        currentStatement.Append(";\r\n");
                    }
                }
                if (currentStatement.Length > 0)
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = currentStatement.ToString();
                    cmd.ExecuteNonQuery();
                    currentStatement.Clear();
                }
            }
        }

        public static IHtValue LowerCaseKeys(IHtValue input)
        {
            var result = new JsonHtValue();
            foreach (var f in input.AsDictionaryElements)
            {
                result[f.Key.ToLowerInvariant()] = f.Value;
            }
            return result;
        }

        public static StringBuilder AppendSqlLiteral(StringBuilder statement, IHtValue val)
        {
            if (val.IsDouble || val.IsLong)
            {
                statement.Append(val.AsString);
            }
            else if (val.IsBool)
            {
                statement.Append(val.AsString.ToUpperInvariant());
            }
            else if (val.IsDictionary)
            {
                DateTime t = new DateTime(val["Ticks"].AsLong);
                statement.Append("TIMESTAMP '");
                statement.Append(t.ToString(SqlDateFormat));
                statement.Append('\'');
            }
            else if (val.IsEmpty)
            {
                statement.Append("null");
            }
            else
            {
                statement.Append('\'');
                foreach (var ch in val.AsString)
                {
                    if (ch == '\'')
                    {
                        statement.Append(ch);
                    }
                    statement.Append(ch);
                }
                statement.Append('\'');
            }
            return statement;
        }

        public static string SingleInsertStatement(IBasicTable table, ISqlColumnInfo columnInfo, IHtValue row)
        {
            StringBuilder statement = new StringBuilder($"INSERT INTO ");
            if (! string.IsNullOrWhiteSpace(table.Schema))
            {
                statement.Append(table.Schema);
                statement.Append('.');
            }
            statement.Append(table.Name);
            statement.Append(" ( ");
            bool first = true;
            foreach (var item in row.AsDictionaryElements)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    statement.Append(',');
                }
                statement.Append(columnInfo.GetColumnReference(table[item.Key]));
            }
            statement.Append(") VALUES (");
            first = true;
            foreach (var item in row.AsDictionaryElements)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    statement.Append(',');
                }
                columnInfo.AppendColumnLiteral(statement, table[item.Key], row[item.Key]);
            }
            statement.Append(')');
            return statement.ToString();
        }

        public static IEnumerable<IHtValue> GetRows(Func<IDataReader, int, IHtValue> getColumnValue, IDbConnection conn, string sql, int commandTimeout)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = commandTimeout;
            using (IDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int count = reader.FieldCount;
                    JsonHtValue val = new JsonHtValue();
                    for (int i = 0; i < count; i++)
                    {
                        string name = reader.GetName(i);
                        if (string.IsNullOrEmpty(name))
                        {
                            name = "Column-" + i;
                        }
                        val[name] = getColumnValue(reader, i);
                    }
                    yield return val;
                }
            }
        }

        public static IEnumerable<IHtValue> GetRows(ISqlConnectionSource connectionSource, string sql, int commandTimeout, Func<IDataReader, int, IHtValue> getColumnValue = null)
        {
            if (getColumnValue == null)
            {
                getColumnValue = GetColumnValue;
            }
            if (connectionSource.Cast<IWorkMonitorSource>() is IWorkMonitorSource monitorSource)
            {
                monitorSource.GetMonitor().Writer.WriteLine(sql);
                monitorSource.GetMonitor().Writer.WriteLine(string.Empty);
            }
            using (var conn = connectionSource.GetConnection())
            {
                foreach (var row in GetRows(getColumnValue, conn, sql, commandTimeout))
                {
                    yield return row;
                }
            }
        }
    }
}

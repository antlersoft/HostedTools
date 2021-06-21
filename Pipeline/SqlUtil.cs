using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace com.antlersoft.HostedTools.Pipeline
{
    public static class SqlUtil
    {
        public static IWorkMonitor Monitor;
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
            else if (t == typeof(long))
            {
                return new JsonHtValue((long)val);
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
            }
            sb.Append(')');
            return sb.ToString();
        }

        public static IEnumerable<IHtValue> GetRows(Func<IDataReader, int, IHtValue> getColumnValue, IDbConnection conn, string sql, int commandTimeout)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (Monitor != null)
            {
                Monitor.Writer.WriteLine(sql);
            }
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
            using (var conn = connectionSource.GetConnection())
            {
                conn.Open();
                foreach (var row in GetRows(getColumnValue, conn, sql, commandTimeout))
                {
                    yield return row;
                }
            }
        }
    }
}

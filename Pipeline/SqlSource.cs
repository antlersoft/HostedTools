using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.Common;

using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IHtValueSource))]
    public class SqlSource : EditOnlyPlugin, ISettingDefinitionSource, IHtValueSource
    {
        static ISettingDefinition SqlCommand = new MultiLineSettingDefinition("SqlCommand", "Pipeline", 8, "Sql Query");
        static ISettingDefinition CommandTimeout = new SimpleSettingDefinition("CommandTimeout", "Pipeline", "Command Timeout", "Number of seconds before query will timeout", typeof(int), "120");

        public SqlSource()
            : base(new MenuItem("DevTools.Pipeline.Input.SqlSource", "Sql Query", typeof(SqlSource).FullName, "DevTools.Pipeline.Input"), new [] {"Common.SqlDataConnectionString", SqlCommand.FullKey(), CommandTimeout.FullKey()} )
        { }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new[] {SqlCommand, CommandTimeout}; }
        }

        private IHtValue GetColumnValue(IDataReader reader, int col)
        {
            object val = reader.GetValue(col);
            Type t = val.GetType();
            if (t == typeof (int))
            {
                return new JsonHtValue((int)val);
            }
            else if (t == typeof (long))
            {
                return new JsonHtValue((long)val);
            }
            else if (t == typeof (double))
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

        public IEnumerable<IHtValue> GetRows()
        {
            using (var conn = DbProviderFactories.GetFactory(SqlDataParameters.DbProviderFactoryName.Value<string>(SettingManager)).CreateConnection())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = SqlSource.SqlCommand.Value<string>(SettingManager);
                cmd.CommandTimeout = CommandTimeout.Value<int>(SettingManager);
                IDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int count = reader.FieldCount;
                    JsonHtValue val = new JsonHtValue();
                    for (int i = 0; i < count; i++)
                    {
                        string name = reader.GetName(i);
                        if (String.IsNullOrEmpty(name))
                        {
                            name = "Column-" + i;
                        }
                        val[name] = GetColumnValue(reader, i);
                    }
                    yield return val;
                }
            }
 
        }

        public string SourceDescription
        {
            get
            {
                string query = SqlCommand.Value<string>(SettingManager);
                int index = query.IndexOf('\n');
                if (index >= 0)
                {
                    query = query.Substring(0, index);
                }
                return "Sql query: "+query;
            }
        }
    }
}

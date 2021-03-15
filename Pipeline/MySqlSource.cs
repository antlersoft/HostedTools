using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using MySqlConnector;
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
    public class MySqlSource : EditOnlyPlugin, IHtValueSource, ISettingDefinitionSource
    {
        static ISettingDefinition MySqlConnectionString = new SimpleSettingDefinition("ConnectionString", "MySqlSource", "Connection string");

        public IEnumerable<ISettingDefinition> Definitions => new [] {MySqlConnectionString};
        public MySqlSource()
        : base(new MenuItem("DevTools.Pipeline.Input.MySqlSource", "MySql Query", typeof(MySqlSource).FullName, "DevTools.Pipeline.Input"), new [] {MySqlConnectionString.FullKey(), SqlSource.SqlCommand.FullKey(), SqlSource.CommandTimeout.FullKey()})
        {

        }

        public IEnumerable<IHtValue> GetRows()
        {
            using (var connection = new MySqlConnection(MySqlConnectionString.Value<string>(SettingManager)))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = SqlSource.SqlCommand.Value<string>(SettingManager);
                cmd.CommandTimeout = SqlSource.CommandTimeout.Value<int>(SettingManager);
                IDataReader reader = cmd.ExecuteReader();
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
                        val[name] = SqlSource.GetColumnValue(reader, i);
                    }
                    yield return val;
                }
               
            }
        }

        public string SourceDescription
        {
            get
            {
                string query = SqlSource.SqlCommand.Value<string>(SettingManager);
                int index = query.IndexOf('\n');
                if (index >= 0)
                {
                    query = query.Substring(0, index);
                }
                return "MySql query: "+query;
            }
        }
    }
}

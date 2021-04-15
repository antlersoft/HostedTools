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
using System.Data.Common;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IHtValueSource))]
    public class MySqlSource : SqlSourceBase, ISettingDefinitionSource
    {
        static ISettingDefinition MySqlConnectionString = new SimpleSettingDefinition("ConnectionString", "MySqlSource", "Connection string");

        public IEnumerable<ISettingDefinition> Definitions => new [] {MySqlConnectionString};
        public MySqlSource()
        : base(new MenuItem("DevTools.Pipeline.Input.MySqlSource", "MySql Query", typeof(MySqlSource).FullName, "DevTools.Pipeline.Input"), new [] {MySqlConnectionString.FullKey(), SqlSource.SqlCommand.FullKey(), SqlSource.CommandTimeout.FullKey()})
        {

        }

        protected override string QueryType => "MySQL";

        protected override DbConnection GetConnection()
        {
            return new MySqlConnection(MySqlConnectionString.Value<string>(SettingManager));
        }
    }
}

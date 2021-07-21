using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Sql.Interface;
using com.antlersoft.HostedTools.Sql.Model;

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

        public override string QueryType => "MySQL";

        public override ISqlConnectionSource GetConnectionSource()
        {
            return new MySqlConnectionSource(MySqlConnectionString.Value<string>(SettingManager));
        }
    }
}

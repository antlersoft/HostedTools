using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Sql.Interface;
using com.antlersoft.HostedTools.Sql.Model;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IRootNode))]
    public class PostgreSqlSource : SqlSourceBase, ISettingDefinitionSource
    {
        static ISettingDefinition PostgreSqlConnectionString = new SimpleSettingDefinition("ConnectionString", "PostgreSqlSource", "Connection string");

        public IEnumerable<ISettingDefinition> Definitions => new[] { PostgreSqlConnectionString };
        public PostgreSqlSource()
        : base(new MenuItem("DevTools.Pipeline.Input.PostgreSqlSource", "PostgreSql Query", typeof(PostgreSqlSource).FullName, "DevTools.Pipeline.Input"), new[] { PostgreSqlConnectionString.FullKey(), SqlSource.SqlCommand.FullKey(), SqlSource.CommandTimeout.FullKey() })
        {

        }

        public override string QueryType => "PostgreSQL";

        public override ISqlConnectionSource GetConnectionSource()
        {
            return new PostgreSqlConnectionSource(PostgreSqlConnectionString.Value<string>(SettingManager), CommandTimeout.Value<int>(SettingManager));
        }
    }
}

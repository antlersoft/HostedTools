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
    public class RedshiftSqlSource : SqlSourceBase, ISettingDefinitionSource
    {
        static ISettingDefinition RedshiftSqlConnectionString = new SimpleSettingDefinition("ConnectionString", "RedshiftSqlSource", "Connection string");

        public IEnumerable<ISettingDefinition> Definitions => new[] { RedshiftSqlConnectionString };
        public RedshiftSqlSource()
        : base(new MenuItem("DevTools.Pipeline.Input.RedshiftSqlSource", "Redshift Sql Query", typeof(RedshiftSqlSource).FullName, "DevTools.Pipeline.Input"), new[] { RedshiftSqlConnectionString.FullKey(), SqlSource.SqlCommand.FullKey(), SqlSource.CommandTimeout.FullKey() })
        {

        }

        public override string QueryType => "Redshift";

        public override ISqlConnectionSource GetConnectionSource()
        {
            return new RedshiftConnectionSource(RedshiftSqlConnectionString.Value<string>(SettingManager), CommandTimeout.Value<int>(SettingManager));
        }
    }
}

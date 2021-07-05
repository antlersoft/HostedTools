using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Sql.Interface;
using com.antlersoft.HostedTools.Sql.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Common;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IHtValueSource))]
    public class PostgreSqlSource : SqlSourceBase, ISettingDefinitionSource, ISqlReferentialConstraintInfo, ISqlPrimaryKeyInfo
    {
        static ISettingDefinition PostgreSqlConnectionString = new SimpleSettingDefinition("ConnectionString", "PostgreSqlSource", "Connection string");

        public IEnumerable<ISettingDefinition> Definitions => new[] { PostgreSqlConnectionString };
        public PostgreSqlSource()
        : base(new MenuItem("DevTools.Pipeline.Input.PostgreSqlSource", "PostgreSql Query", typeof(PostgreSqlSource).FullName, "DevTools.Pipeline.Input"), new[] { PostgreSqlConnectionString.FullKey(), SqlSource.SqlCommand.FullKey(), SqlSource.CommandTimeout.FullKey() })
        {

        }

        public override string QueryType => "PostgreSQL";

        public override DbConnection GetConnection()
        {
            return new PostgreSqlConnectionSource(PostgreSqlConnectionString.Value<string>(SettingManager), CommandTimeout.Value<int>(SettingManager)).GetConnection();
        }

        public override Dictionary<string, IIndexSpec> GetIndexInfo(IBasicTable table)
        {
            return new PostgreSqlConnectionSource(PostgreSqlConnectionString.Value<string>(SettingManager), CommandTimeout.Value<int>(SettingManager)).GetIndexInfo(table);
        }

        public IEnumerable<IConstraint> GetReferentialConstraints(IBasicTable table, Func<string, string, ITable> tableGetter)
        {
            return new PostgreSqlConnectionSource(PostgreSqlConnectionString.Value<string>(SettingManager), CommandTimeout.Value<int>(SettingManager)).GetReferentialConstraints(table, tableGetter);
        }

        public IIndexSpec GetPrimaryKey(IBasicTable table)
        {
            return new PostgreSqlConnectionSource(PostgreSqlConnectionString.Value<string>(SettingManager), CommandTimeout.Value<int>(SettingManager)).GetPrimaryKey(table);
        }
    }
}

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
    public class SQLiteSource : SqlSourceBase, ISettingDefinitionSource
    {
        static ISettingDefinition SQLiteConnectionString = new SimpleSettingDefinition("ConnectionString", "SQLiteSource", "Path to SQLite db");

        public IEnumerable<ISettingDefinition> Definitions => new[] { SQLiteConnectionString };
        public SQLiteSource()
        : base(new MenuItem("DevTools.Pipeline.Input.SQLiteSource", "SQLite Query", typeof(SQLiteSource).FullName, "DevTools.Pipeline.Input"), new[] {SQLiteConnectionString.FullKey(), SqlSource.SqlCommand.FullKey() })
        {

        }

        public override string QueryType => "SQLite";

        public override ISqlConnectionSource GetConnectionSource()
        {
            return new SQLiteConnectionSource(SQLiteConnectionString.Value<string>(SettingManager));
        }
    }
}
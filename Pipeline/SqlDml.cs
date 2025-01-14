using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Common;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;

namespace com.antlersoft.HostedTools.Pipeline {
    [Export(typeof(IMenuItemSource))]
    public class DmlMenuPart : IMenuItemSource
    {
        public static readonly string DmlMenuKey = "DevTools.Dml";
        IMenuItem[] DmlMenuBase = new [] { new MenuItem(DmlMenuKey, "Sql DML", null, "DevTools") };
        public IEnumerable<IMenuItem> Items => DmlMenuBase;
    }

    /// <summary>
    /// Base class for tools for running DML statements with DbConnection
    /// </summary>
    [InheritedExport(typeof(ISettingDefinitionSource))]
    public abstract class SqlDml : SimpleWorker, ISettingDefinitionSource 
    {
        [Import]
        public IPluginManager PluginManager {get;set;}
        protected ISettingDefinition[] settingDefinitions;
        protected readonly string _suffix;
        protected readonly string _sourcePlugin;
        private SqlSourceBase _sqlSourceBase;

        protected static string GetActionId(string suffix) {
            return $"{typeof(SqlDml).FullName}.{suffix}";
        }
        public SqlDml(string suffix, string menuPrompt, ISettingDefinition connectionSetting, string sourcePlugin)
        : base(new MenuItem(suffix+"."+DmlMenuPart.DmlMenuKey, menuPrompt, GetActionId(suffix), DmlMenuPart.DmlMenuKey), new [] { connectionSetting.FullKey(), "SqlDml"+suffix+".Dml" })
        {
            _suffix = suffix;
            settingDefinitions = new [] {new MultiLineSettingDefinition("Dml", "SqlDml"+suffix, 6, "DML", "Data manipulation language statement for "+suffix)};
            _sourcePlugin = sourcePlugin;
        }

        public override string Name => GetActionId(_suffix);

        public IEnumerable<ISettingDefinition> Definitions => settingDefinitions;

        public virtual DbConnection GetConnection() {
            if (_sqlSourceBase == null) {
                _sqlSourceBase = PluginManager[_sourcePlugin].Cast<SqlSourceBase>();
            }
            return _sqlSourceBase.GetConnectionSource().GetConnection();
        }

        public override void Perform(IWorkMonitor monitor)
        {
            using (var connection = GetConnection()) {
                var statement = settingDefinitions[0].Value<string>(SettingManager);
                using (var cmd = connection.CreateCommand()) {
                    cmd.CommandText = statement;
                    int rows = cmd.ExecuteNonQuery();
                    monitor.Writer.WriteLine($"{rows} rows affected");
                }
            }
        }
    }

    public class SQLiteDml : SqlDml {
        public SQLiteDml()
        : base("SQLite", "SQLite Non-Query", SQLiteSource.SQLiteConnectionString, typeof(SQLiteSource).FullName)
        {}
    }
    public class MySqlDml : SqlDml {
        public MySqlDml()
        : base("MySql", "MySql Non-Query", MySqlSource.MySqlConnectionString, typeof(MySqlSource).FullName)
        {}
    }

    public class PostgresDml : SqlDml {
        public PostgresDml()
        :base("PostgresSql", "Postgres Non-Query", PostgreSqlSource.PostgreSqlConnectionString, typeof(PostgreSqlSource).FullName)
        {

        }
    }
}
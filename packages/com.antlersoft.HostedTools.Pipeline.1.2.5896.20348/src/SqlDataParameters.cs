
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;

namespace com.antlersoft.HostedTools.Pipeline
{
    /// <summary>
    /// Default SQL connection string; also defines top menu items
    /// </summary>
    public class SqlDataParameters : EditingSettingDeclarer
    {
        public static ISettingDefinition DataConnectionString = new SimpleSettingDefinition("SqlDataConnectionString",
                                                                                         "Common",
                                                                                         "SQL Data Connection String",
                                                                                         "MS-SQL connection string for database");
        public SqlDataParameters()
            : base(new[] { new MenuItem("DevTools", "Developer Tools"), new MenuItem("Common", "Common"), new MenuItem("Common.SqlDataParameters", "Sql Connection String", typeof(SqlDataParameters).FullName, "Common")  }, new[] { DataConnectionString })
        { }
    }

}

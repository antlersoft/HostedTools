﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.Common;

using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Sql;
using com.antlersoft.HostedTools.Sql.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    public abstract class SqlSourceBase : EditOnlyPlugin, IHtValueSource
    {
        protected SqlSourceBase(IEnumerable<IMenuItem> menuEntries, IEnumerable<string> keys)
        : base(menuEntries, keys)
        {}
        protected SqlSourceBase(IMenuItem item, IEnumerable<string> keys)
        : base(item, keys)
        {}

        public abstract string QueryType { get; }

        protected virtual IHtValue GetColumnValue(IDataReader reader, int col)
        {
            return SqlUtil.GetColumnValue(reader, col);
        }

        /// <summary>
        /// Return a new instance of an ISqlConnectionSource appropriate to this SQL type and connection info
        /// </summary>
        /// <returns>A new instance of an ISQLConnectionSource</returns>
        public abstract ISqlConnectionSource GetConnectionSource();

        public virtual IEnumerable<IHtValue> GetRows()
        {
            return SqlUtil.GetRows(GetConnectionSource(), SqlCommand.Value<string>(SettingManager), CommandTimeout.Value<int>(SettingManager), GetColumnValue);
        }

        public virtual string SourceDescription
        {
            get
            {
                string query = SqlCommand.Value<string>(SettingManager);
                int index = query.IndexOf('\n');
                if (index >= 0)
                {
                    query = query.Substring(0, index);
                }
                return $"{QueryType}: {query}";
            }
        }

        protected static ISettingDefinition SqlCommand = new MultiLineSettingDefinition("SqlCommand", "Pipeline", 8, "Sql Query");
        protected static ISettingDefinition CommandTimeout = new SimpleSettingDefinition("CommandTimeout", "Pipeline", "Command Timeout", "Number of seconds before query will timeout", typeof(int), "120");
    }

    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IHtValueSource))]
    public class SqlSource : SqlSourceBase, ISettingDefinitionSource, IHtValueSource
    {

        public SqlSource()
            : base(new MenuItem("DevTools.Pipeline.Input.SqlSource", "Sql Query", typeof(SqlSource).FullName, "DevTools.Pipeline.Input"), new [] {"Common.SqlDataConnectionString", SqlCommand.FullKey(), CommandTimeout.FullKey()} )
        { }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new[] {SqlCommand, CommandTimeout}; }
        }

        public override ISqlConnectionSource GetConnectionSource()
        {
            return new SqlConnectionSource(SqlDataParameters.DbProviderFactoryName.Value<string>(SettingManager));
        }

        public override string QueryType => "SQL Server";
    }

    public class SqlConnectionSource : HostedObjectBase, ISqlConnectionSource
    {
        private string _factoryName;
        internal SqlConnectionSource(string factoryName)
        {
            _factoryName = factoryName;
        }
        public virtual DbConnection GetConnection()
        {
            return DbProviderFactories.GetFactory(_factoryName).CreateConnection();
        }
    }

}

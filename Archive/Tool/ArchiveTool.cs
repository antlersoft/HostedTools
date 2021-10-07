using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.Archive.Model;
using com.antlersoft.HostedTools.Archive.Model.Configuration;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Pipeline;
using com.antlersoft.HostedTools.Serialization;
using com.antlersoft.HostedTools.Sql.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;

namespace com.antlersoft.HostedTools.Archive.Tool
{

    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IAfterComposition))]
    public class ArchiveTool : SimpleWorker, ISettingDefinitionSource, IAfterComposition
    {
        internal static PluginSelectionSettingDefinition SqlSources = new PluginSelectionSettingDefinition(PluginDescription, "SqlSources", "Archive", "ArchiveTool");
        internal static PathSettingDefinition RepositoryConfigurationJson = new PathSettingDefinition("RepositoryConfigurationJson", "Archive", "Repository Configuration Json", false, false, ".json|*.json");
        internal static ISettingDefinition RepoFolder = new PathSettingDefinition("RepoFolder", "Archive", "Repositiory Folder", true, true);
        internal static ISettingDefinition TableSpecs = new MultiLineSettingDefinition("TableSpecs", "Archive", 8, "Table specs", "Paired lines of table name/expression");
        internal static ISettingDefinition ArchiveTitle = new SimpleSettingDefinition("Title", "Archive", "Title");

        public IEnumerable<ISettingDefinition> Definitions => new[] { SqlSources, RepositoryConfigurationJson, RepoFolder, TableSpecs, ArchiveTitle };

        [ImportMany]
        public IEnumerable<IHtValueSource> ValueSources { get; set; }

        [Import]
        public IJsonFactory JsonFactory { get; set; }

        private static string PluginDescription(IPlugin plugin)
        {
            return plugin.Cast<SqlSourceBase>().QueryType;
        }

        private WorkMonitorSource monitorSource = new WorkMonitorSource();
        public ArchiveTool()
        : base(new MenuItem[] {new MenuItem("Archive", "Archive"), new MenuItem("Archive.ArchiveTool", "Create folder archive from SQL", typeof(ArchiveTool).FullName, "Archive")}, new[] { SqlSources.FullKey(), RepositoryConfigurationJson.FullKey(), RepoFolder.FullKey(), TableSpecs.FullKey(), ArchiveTitle.FullKey()})
        {
            RepositoryConfigurationJson.InjectImplementation(typeof(IEditablePath), new EditablePath());
        }
        public override void Perform(IWorkMonitor monitor)
        {
            var configPath = RepositoryConfigurationJson.Value<string>(SettingManager);
            var repoPath = RepoFolder.Value<string>(SettingManager);
            var tableSpecs = TableSpecs.Value<string>(SettingManager);
            var title = ArchiveTitle.Value<string>(SettingManager);
            SqlRepositoryConfiguration config = null;
            using (var reader = new StreamReader(configPath))
            using (var jreader = new JsonTextReader(reader))
            {
                config = JsonFactory.GetSerializer().Deserialize<SqlRepositoryConfiguration>(jreader);
            }
            var sqlModule = (SqlSources.FindMatchingItem(SqlSources.Value<string>(SettingManager)) as PluginSelectionItem).Plugin as SqlSourceBase;
            ISqlConnectionSource connectionSource = sqlModule.GetConnectionSource();
            if (connectionSource.Cast<IAggregator>() is IAggregator agg)
            {
                agg.InjectImplementation(typeof(IWorkMonitorSource), monitorSource);
            }
            monitorSource.SetMonitor(monitor);
            var cancelable = monitor.Cast<ICancelableMonitor>();
            var token = cancelable == null ? CancellationToken.None : cancelable.Cancellation;
            if (monitor.Cast<IBackgroundableMonitor>() is IBackgroundableMonitor backgroundable)
            {
                backgroundable.CanBackground($"{sqlModule.QueryType} Title: {title}");
            }
            SqlRepository sr = new SqlRepository(config, connectionSource);
            FolderRepository fr = new FolderRepository(repoPath, sr.Schema);
            var cb = new HostedTools.ConditionBuilder.Model.ConditionBuilder();
            ITable table = null;
            var specs = new List<IArchiveTableSpec>();
            foreach (var line in tableSpecs.Split('\n'))
            {
                if (table == null)
                {
                    string[] elements = line.Split('.');
                    if (elements.Length == 1)
                    {
                        table = sr.Schema.GetTable(config.DefaultSchema ?? string.Empty, elements[0]);
                    }
                    else
                    {
                        table = sr.Schema.GetTable(elements[0], elements[1]);
                    }
                    if (table==null)
                    {
                        throw new Exception("Table not found for: " + line);
                    }
                }
                else
                {
                    var backPosition = line.IndexOf("back:");
                    string expressionText;
                    var backReferences = new List<ITable>();
                    if (backPosition >= 0)
                    {
                        expressionText = line.Substring(0, backPosition);
                        foreach (var tableString in line.Substring(backPosition+5).Split(','))
                        {
                            var ts = tableString.Trim();
                            if (! string.IsNullOrEmpty(ts))
                            {
                                string[] elements = ts.Split('.');
                                ITable backTable;
                                if (elements.Length == 1)
                                {
                                    backTable = sr.Schema.GetTable(config.DefaultSchema ?? string.Empty, elements[0]);
                                }
                                else
                                {
                                    backTable = sr.Schema.GetTable(elements[0], elements[1]);
                                }
                                if (table != null)
                                {
                                    backReferences.Add(backTable);
                                }
                            }
                        }
                    }
                    else
                    {
                        expressionText = line;
                    }
                    var expression = string.IsNullOrWhiteSpace(expressionText) ? null : new ExpressionWithSource(cb, expressionText);
                    specs.Add(new ArchiveTableSpec(table, expression, backReferences));
                    table = null;
                }
            }
            if (table != null)
            {
                specs.Add(new ArchiveTableSpec(table, null));
            }
            var archive = sr.GetArchive(new ArchiveSpec(specs, title), monitor);
            if (token.IsCancellationRequested)
            {
                return;
            }
            fr.WriteArchive(archive, monitor);
        }

        public void AfterComposition()
        {
            SqlSources.SetPlugins(ValueSources.Where(t => t is SqlSourceBase).Select(t => t.Cast<IPlugin>()).ToList(), SettingManager);
        }
    }

    public class LoadArchiveTool : SimpleWorker
    {
        [Import]
        public IJsonFactory JsonFactory;

        private WorkMonitorSource _monitorSource = new WorkMonitorSource();
        public LoadArchiveTool()
        : base(new MenuItem("Archive.Load", "Load archive into SQL repository", typeof(LoadArchiveTool).FullName, "Archive"),
        new [] { ArchiveTool.RepoFolder.FullKey(), ArchiveTool.SqlSources.FullKey(), ArchiveTool.RepositoryConfigurationJson.FullKey(),
        ArchiveTool.ArchiveTitle.FullKey()})
        { }

        public override void Perform(IWorkMonitor monitor)
        {
            var configPath = ArchiveTool.RepositoryConfigurationJson.Value<string>(SettingManager);
            var repoPath = ArchiveTool.RepoFolder.Value<string>(SettingManager);
            var title = ArchiveTool.ArchiveTitle.Value<string>(SettingManager);
            SqlRepositoryConfiguration config = null;
            using (var reader = new StreamReader(configPath))
            using (var jreader = new JsonTextReader(reader))
            {
                config = JsonFactory.GetSerializer().Deserialize<SqlRepositoryConfiguration>(jreader);
            }
            var sqlModule = (ArchiveTool.SqlSources.FindMatchingItem(ArchiveTool.SqlSources.Value<string>(SettingManager)) as PluginSelectionItem).Plugin as SqlSourceBase;
            ISqlConnectionSource connectionSource = sqlModule.GetConnectionSource();
            if (connectionSource.Cast<IAggregator>() is IAggregator agg)
            {
                agg.InjectImplementation(typeof(IWorkMonitorSource), _monitorSource);
            }
            _monitorSource.SetMonitor(monitor);

            var cancelable = monitor.Cast<ICancelableMonitor>();
            var token = cancelable == null ? CancellationToken.None : cancelable.Cancellation;
            if (monitor.Cast<IBackgroundableMonitor>() is IBackgroundableMonitor backgroundable)
            {
                backgroundable.CanBackground($"{sqlModule.QueryType} Title: {title}");
            }
            SqlRepository sr = new SqlRepository(config, connectionSource);
            FolderRepository fr = new FolderRepository(repoPath, sr.Schema);
            var spec = fr.AvailableArchives().FirstOrDefault(a => a.Title == title);
            if (spec == null)
            {
                monitor.Writer.WriteLine($"No archive with title [{title}] found");
                return;
            }
            sr.WriteArchive(fr.GetArchive(spec, monitor), monitor);
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IAfterComposition))]
    [Export(typeof(IMenuItemSource))]
    public class NoSqlSourceDefinitions : HostedObjectBase, ISettingDefinitionSource, IAfterComposition, IMenuItemSource
    {
        [Import] public ISettingManager SettingManager;
        [ImportMany] public INoSqlSource[] Sources;
        private PluginSelectionSettingDefinition _source;
        private PluginSelectionSettingDefinition _target;

        public static string SourceKey = "NoSqlSourceDefinitions.SourcePlugin";
        public static string TargetKey = "NoSqlSourceDefinitions.TargetPlugin";

        public NoSqlSourceDefinitions()
        {
            _source = new PluginSelectionSettingDefinition(GetDescription, "SourcePlugin", "NoSqlSourceDefinitions", "NoSql Source", "Select a NoSql source from the list");
            _target = new PluginSelectionSettingDefinition(GetDescription, "TargetPlugin", "NoSqlSourceDefinitions", "NoSql Target", "Select a NoSql target from the list");
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new[] {_source, _target}; }
        }

        public void AfterComposition()
        {
            var pluginList = Sources.Select(s => s.Cast<IPlugin>()).ToList();
            _source.SetPlugins(pluginList, SettingManager);
            _target.SetPlugins(pluginList, SettingManager);
        }

        private string GetDescription(IPlugin noSqlPlugin)
        {
            return noSqlPlugin.Cast<INoSqlSource>().Description;
        }

        public static INoSqlSource GetNoSqlFromDefinition(ISetting setting)
        {
            return ((PluginSelectionItem)setting.Definition.Cast<PluginSelectionSettingDefinition>().FindMatchingItem(setting.GetRaw())).Plugin.Cast<INoSqlSource>();
        }

        public IEnumerable<IMenuItem> Items
        {
            get { return new [] {new MenuItem("DevTools.NoSqlSources", "NoSql Sources", null, "DevTools")}; }
        }
    }
}

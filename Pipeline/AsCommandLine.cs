using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Serialization;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IAfterComposition))]
    [Export(typeof(ISettingDefinitionSource))]
    public class AsCommandLine : SimpleWorker, IAfterComposition, ISettingDefinitionSource
    {
        [Import]
        public IPluginManager PluginManager { get; set; }

        [Import]
        public IMenuManager MenuManager { get; set; }

        [Import]
        public IJsonFactory JsonFactory;

        PluginSelectionSettingDefinition _pluginSelectionDefinition;

        MenuIdentifiedPluginDescription _pluginDescription = new MenuIdentifiedPluginDescription();

        public AsCommandLine()
        : base(new MenuItem("Common.AsCommandLine", "Show command-line to run plugin", typeof(AsCommandLine).FullName, "Common"), new [] {"Common.AsCommandPlugin"})
        {
            _pluginSelectionDefinition = new PluginSelectionSettingDefinition(_pluginDescription.GetPluginDescription, "AsCommandPlugin", "Common", "Item to show command-line for");
        }

        public void AfterComposition()
        {
            _pluginDescription.AfterComposition(MenuManager,SettingManager,PluginManager.Plugins.Where(p => p is IWork),_pluginSelectionDefinition);
        }

        public IEnumerable<ISettingDefinition> Definitions => new ISettingDefinition[] { _pluginSelectionDefinition };

        private string EscapeString(string raw)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('\'');
            foreach (char ch in raw)
            {
                switch (ch)
                {
                    case '\n' :
                        sb.Append('\\');
                        sb.Append('n');
                        break;
                    case '\'':
                        sb.Append('\\');
                        sb.Append('\'');
                        break;
                    default :
                        sb.Append(ch);
                        break;
                }
            }
            sb.Append('\'');
            return sb.ToString();
        }

        public override void Perform(IWorkMonitor monitor)
        {
            monitor.Writer.Write($"dotnet CommandHost.dll ");
            string pluginName = _pluginSelectionDefinition.Value<string>(SettingManager);
            IPlugin plugin = PluginManager.Plugins.First(p => p.Name == pluginName);
            if (plugin.Cast<IWorkNode>() is IWorkNode workNode) {
                monitor.Writer.Write($"--workNode ");
                StringWriter sw = new StringWriter();
                JsonFactory.GetSerializer().Serialize(sw, workNode.GetPluginState());
                monitor.Writer.Write(EscapeString(sw.ToString()));
                monitor.Writer.Write(" ");
            }
            else if (plugin.Cast<ISettingEditList>() is ISettingEditList list)
            {
                foreach (var setting in list.KeysToEdit.Select(k => SettingManager[k]))
                {
                    monitor.Writer.Write($"--{setting.Definition.FullKey()} {EscapeString(setting.GetRaw())} ");
                }
            }
            monitor.Writer.WriteLine(plugin.Name);
        }
    }
}
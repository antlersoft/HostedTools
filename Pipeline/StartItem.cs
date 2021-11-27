using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IAfterComposition))]
    public class StartItem : EditingSettingDeclarer, IAfterComposition
    {
        ISettingDefinition UserStartItem = new SimpleSettingDefinition("UseStartItem", "Common", "Use a specific start page", null, typeof(bool), "false", false, 0);
        ISettingDefinition TextEditorDefinition = new PathSettingDefinition("TextEditor", "Common", "Text Editor", false, false, null, "Path to editor to use for text files", "/usr/bin/vim");
        PluginSelectionSettingDefinition _pluginSelectionDefinition;

        [Import]
        public IPluginManager PluginManager { get; set; }

        [Import]
        public IMenuManager MenuManager { get; set; }

        [Import]
        public ISettingManager SettingManager { get; set; }

        private MenuIdentifiedPluginDescription _pluginDescription = new MenuIdentifiedPluginDescription();

        public StartItem()
        : base(new [] {new MenuItem("Common.StartItem", "Set Start Item", typeof(StartItem).FullName, "Common")}, new ISettingDefinition[0])
        {
            _pluginSelectionDefinition = new PluginSelectionSettingDefinition(_pluginDescription.GetPluginDescription, "StartItem", "Common", "Set item to go to at start-up");
        }

        public override IEnumerable<ISettingDefinition> Definitions => new[] { UserStartItem, _pluginSelectionDefinition, TextEditorDefinition };

        public override IEnumerable<string> KeysToEdit => Definitions.Select(d => d.FullKey());
        public void AfterComposition()
        {
            _pluginDescription.AfterComposition(MenuManager,SettingManager,PluginManager.Plugins, _pluginSelectionDefinition);
        }
    }
}

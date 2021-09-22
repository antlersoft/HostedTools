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

        public Dictionary<IPlugin, string> _keyToDescriptionDictionary = new Dictionary<IPlugin, string>();

        public StartItem()
        : base(new [] {new MenuItem("Common.StartItem", "Set Start Item", typeof(StartItem).FullName, "Common")}, new ISettingDefinition[0])
        {
            _pluginSelectionDefinition = new PluginSelectionSettingDefinition(GetPluginDescription, "StartItem", "Common", "Set item to go to at start-up");
        }

        public string GetPluginDescription(IPlugin plugin)
        {
            string result;
            if (! _keyToDescriptionDictionary.TryGetValue(plugin, out result))
            {
                result = string.Empty;
            }
            return result;
        }

        public override IEnumerable<ISettingDefinition> Definitions => new[] { UserStartItem, _pluginSelectionDefinition, TextEditorDefinition };

        public override IEnumerable<string> KeysToEdit => Definitions.Select(d => d.FullKey());
        public void AfterComposition()
        {
            var actions = new Dictionary<string,IMenuItem>();
            var items = MenuManager.GetChildren(null).ToList();
            for (int i = 0; i<items.Count; i++)
            {
                var item = items[i];
                if (! string.IsNullOrEmpty(item.ActionId))
                {
                    actions[item.ActionId] = item;
                }
                items.AddRange(MenuManager.GetChildren(item));
            }
            foreach (var plugins in PluginManager.Plugins)
            {
                IMenuItem item;
                if (actions.TryGetValue(plugins.Name, out item))
                {
                    _keyToDescriptionDictionary[plugins] = item.GetBreadCrumbString(MenuManager);   
                }
            }
            var pluginList = _keyToDescriptionDictionary.Keys.OrderBy(p => _keyToDescriptionDictionary[p]).ToList();
            _pluginSelectionDefinition.SetPlugins(pluginList, SettingManager);
        }
    }
}

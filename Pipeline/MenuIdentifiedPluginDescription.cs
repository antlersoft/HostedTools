using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using System.Collections.Generic;
using System.Linq;

namespace com.antlersoft.HostedTools.Pipeline
{
    public class MenuIdentifiedPluginDescription
    {
        public Dictionary<IPlugin, string> _keyToDescriptionDictionary = new Dictionary<IPlugin, string>();

        public string GetPluginDescription(IPlugin plugin)
        {
            string result;
            if (! _keyToDescriptionDictionary.TryGetValue(plugin, out result))
            {
                result = string.Empty;
            }
            return result;
        }

        public void AfterComposition(IMenuManager menuManager, ISettingManager settingManager, IEnumerable<IPlugin> pluginSource,
            PluginSelectionSettingDefinition pluginSelectionDefinition)
        {
            var actions = new Dictionary<string,IMenuItem>();
            var items = menuManager.GetChildren(null).ToList();
            for (int i = 0; i<items.Count; i++)
            {
                var item = items[i];
                if (! string.IsNullOrEmpty(item.ActionId))
                {
                    actions[item.ActionId] = item;
                }
                items.AddRange(menuManager.GetChildren(item));
            }
            foreach (var plugins in pluginSource)
            {
                IMenuItem item;
                if (actions.TryGetValue(plugins.Name, out item))
                {
                    _keyToDescriptionDictionary[plugins] = item.GetBreadCrumbString(menuManager);   
                }
            }
            var pluginList = _keyToDescriptionDictionary.Keys.OrderBy(p => _keyToDescriptionDictionary[p]).ToList();
            pluginSelectionDefinition.SetPlugins(pluginList, settingManager);
        }
}
}
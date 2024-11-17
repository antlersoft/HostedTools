using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Setting;

namespace com.antlersoft.HostedTools.Pipeline
{
    public class PluginSelectionItem : ItemSelectionItem, INotifyPropertyChanged
    {
        public IPlugin Plugin { get; private set; }
        internal Func<IPlugin, string> _toStringFunc;

        internal PluginSelectionItem(IPlugin plugin, ISettingManager settingManager, Func<IPlugin,string> toStringFunc)
        {
            Plugin = plugin;
            _toStringFunc = toStringFunc;
            ISettingEditList settings = plugin.Cast<ISettingEditList>();
            if (settings != null)
            {
                foreach (var setting in settings.KeysToEdit)
                {
                    settingManager[setting].SettingChangedListeners.AddListener(UpdateAction);
                }
            }
        }

        private void UpdateAction(ISetting setting)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(null));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public override string ItemDescription
        {
            get {
                string result = _toStringFunc(Plugin);
                if (result.Length > 100) {
                    result = result.Substring(0,97)+"...";
                }
                return result;
            }
        }
    }

    public class PluginSelectionSettingDefinition : SimpleSettingDefinition, IItemSelectionDefinition
    {
        private IList<IPlugin> _plugins;
        private List<PluginSelectionItem> _items;
        private Func<IPlugin, string> _toStringFunc; 
 
        public PluginSelectionSettingDefinition(Func<IPlugin,string> toStringFunc, string id, string scope, string prompt, string description = null)
            : base(id, scope, prompt, description, typeof (string), null, false, 0)
        {
            _toStringFunc = toStringFunc;
        }

        public void SetPlugins(IList<IPlugin> plugins, ISettingManager settingManager)
        {
            _plugins = plugins;
            _items = _plugins.Select(p => new PluginSelectionItem(p, settingManager, _toStringFunc
                )).ToList();
        }

        public object FindMatchingItem(string rawText)
        {
            return _items.FirstOrDefault(i => i.Plugin.Name == rawText);
        }

        public string GetRawTextForItem(object item)
        {
            if (item == null)
            {
                return string.Empty;
            }
            return ((PluginSelectionItem) item).Plugin.Name;
        }

        public IEnumerable<object> GetAllItems()
        {
            return _items;
        }

        public bool IncludeEditButton()
        {
            return true;
        }

        public string NavigateToOnEdit(object item)
        {
            return ((PluginSelectionItem)item).Plugin.Name;
        }
    }
}

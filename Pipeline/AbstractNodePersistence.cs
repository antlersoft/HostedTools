using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.IO;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Navigation;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.Pipeline {
    class NodeItem<T> : ItemSelectionItem where T : class, IPipelineNode {
        internal SavedNode _node;
        internal T _plugin;
        public override string ItemDescription {
            get {
                var origState = _plugin.GetPluginState();
                _plugin.SetPluginState(_node.State);
                string text = $"{_node.Name} - {_plugin.NodeDescription}";
                _plugin.SetPluginState(origState);
                if (text.Length > 190) {
                    text = text.Substring(0,187)+"...";
                }
                return text;
            }
        }

        internal NodeItem(SavedNode node, AbstractNodePersistence<T> container) {
            _node = node;
            _plugin = container.PluginManager[node.State.PluginName].Cast<T>();
        }

        public override bool Equals(object obj)
        {
            if (obj is NodeItem<T> node) {
                return _node.State.Key == node._node.State.Key;
            }
            return false;
        }
    }
    class SavedItemSelection<T> : SimpleSettingDefinition, IItemSelectionDefinition
        where T : class, IPipelineNode
    {
        AbstractNodePersistence<T> _container;
        List<NodeItem<T>> _allItems=new List<NodeItem<T>>();
        internal SavedItemSelection(AbstractNodePersistence<T> container, string name, string scope)
        : base(name, scope, "Saved item names", "All the saved values for type", typeof(string), null, false, 0) {
            _container = container;
        }
        public object FindMatchingItem(string rawText)
        {
            return _allItems.FirstOrDefault(n => n._node.State.Key == rawText);
        }

        public IEnumerable<object> GetAllItems()
        {
            _allItems.Clear();
            foreach (var s in _container.NodeStore.GetMatching(typeof(T), "")) {
                _allItems.Add(new NodeItem<T>(s, _container));
            }
            return _allItems;
        }

        public string GetRawTextForItem(object item)
        {
            return ((NodeItem<T>)item)._node.State.Key;
        }

        public bool IncludeEditButton()
        {
            return false;
        }

        public string NavigateToOnEdit(object item)
        {
            throw new NotImplementedException();
        }
    }

    [InheritedExport(typeof(ISettingDefinitionSource))]
    [InheritedExport(typeof(IMenuItemSource))]
    [InheritedExport(typeof(IPlugin))]
    [InheritedExport(typeof(IAfterComposition))]
    public abstract class AbstractNodePersistence<T> : HostedObjectBase, IPlugin, IPipelineNode,
        ISettingEditList, ISettingDefinitionSource, IMenuItemSource, IHasSettingChangeActions, IAfterComposition
        where T : class, IPipelineNode
    {
        protected abstract string NodeName { get; }
        protected abstract string MenuPrefixKey {get;}
        [Import]
        public IPluginManager PluginManager { get; set;}
        [Import]
        public INodeStore NodeStore { get; set;}
        [Import]
        public ISettingManager SettingManager {get;set;}
        [Import]
        public IJsonFactory JsonFactory {get; set;}
        [Import]
        public INavigationManager NavigationManager {get; set;}

        private PluginSelectionSettingDefinition _pluginSelection;
        private SimpleSettingDefinition _nameSetting;
        private SimpleSettingDefinition _existing;
        private SimpleSettingDefinition _description;
        private ButtonsDefinition _editorButtons, _itemButtons;
        private SavedItemSelection<T> _savedItems;
        private IMenuItem[] _menuItems;
        private ISettingDefinition[] _settingDefinitions;
        private string[] _editable;
        private Dictionary<string, Action<IWorkMonitor, ISetting>> _actionsBySettingKey;
        private bool _initialized = false;
        private string _scopeName => "Pipeline.Persistence."+NodeName;

        private D DeserializeString<D>(string s) {
            if (string.IsNullOrWhiteSpace(s)) {
                return default(D);
            }
            using (var sr=new StringReader(s))
            using (var jtr=new JsonTextReader(sr)) {
                return JsonFactory.GetSerializer().Deserialize<D>(jtr);
            }
        }

        private string SerializeObject(object o) {
            string result = null; 
            if (o != null) {
                using (var sr = new StringWriter()) {
                    JsonFactory.GetSerializer().Serialize(sr, o);
                    result = sr.ToString();
                }
            }
            return result;
        }

        protected static readonly string UpdateOrRename = "Update and/or rename selected with editor content";
        protected static readonly string PersistNew = "Persist editor content as new";
        protected static readonly string SelectCurrent = "Select current from editor";

        protected static readonly string EditButton = "Edit selected item in editor";
        protected static readonly string RenameSelected = "Update (just the) name of selected item";
        protected static readonly string DeleteButton = "Delete";

        protected virtual SavedNode GetNodeToSave(IWorkMonitor wm) {
            string name = _nameSetting.Value<string>(SettingManager);
            string state = _existing.Value<string>(SettingManager);
            if (string.IsNullOrWhiteSpace(state)) {
                wm.Writer.WriteLine("No item state selected to save");
                return null;
            }
            if (string.IsNullOrWhiteSpace(name)) {
                wm.Writer.WriteLine("Can't save with empty name");
                return null;
            }
            SavedNode sn = new SavedNode();
            sn.Name = name;
            sn.State = DeserializeString<PluginState>(state);
            return sn;
        }
        private AbstractNodePersistence<T> initialize() {
            lock (this) {
                if (! _initialized) {
                    _initialized = true;
                    var prefix = MenuPrefixKey;
                    if ( prefix.Length > 0 ) {
                        prefix = "." + prefix;
                    }
                    _menuItems = new IMenuItem[]
                    {
                        new MenuItem($"DevTools.Pipeline{prefix}.Persist", "Persist named " + NodeName,
                        Name, "DevTools.Pipeline" + prefix)
                    };
                    _pluginSelection = new PluginSelectionSettingDefinition(PipelinePlugin.NodeFunc<T>,
                        "PluginSelection", _scopeName, NodeName + " editor",
                        "Select one of the "+NodeName+" editors to persist");
                    _nameSetting = new SimpleSettingDefinition("SavedNodeName", _scopeName,
                        "Name", "Name to associated with persisted "+NodeName, typeof(string));
                    _existing = new SimpleSettingDefinition("Existing", _scopeName, null, null, typeof(string), null, false, 0);
                    _editorButtons = new ButtonsDefinition("EditorButtons", _scopeName, new [] { UpdateOrRename, PersistNew, SelectCurrent});
                    _itemButtons = new ButtonsDefinition("ItemButtons", _scopeName, new [] { EditButton, RenameSelected, DeleteButton});
                    _savedItems = new SavedItemSelection<T>(this, "SavedItems", _scopeName);
                    _description = new SimpleSettingDefinition("Description", _scopeName, "Selected", "Description of Currently Selected "+NodeName, null, null, false, 0);
                    _settingDefinitions=new ISettingDefinition[] {
                        _pluginSelection, _nameSetting, _existing, _editorButtons, _itemButtons, _savedItems, _description
                    };
                    _editable = new [] {
                        _description.FullKey(), _nameSetting.FullKey(), _pluginSelection.FullKey(), _editorButtons.FullKey(), _savedItems.FullKey(), _itemButtons.FullKey()
                    };
                    _actionsBySettingKey = new Dictionary<string, Action<IWorkMonitor, ISetting>> {
                        { _editorButtons.FullKey(), (m, s) => {
                                var button = s.Get<string>();
                                var stateToSave = PluginManager[_pluginSelection.Value<string>(SettingManager)]?.Cast<T>()?.GetPluginState();
                                if (stateToSave == null) {
                                    m.Writer.WriteLine("Can't figure out what editor data to use");
                                    return;
                                }
                                if (button==PersistNew) {
                                    stateToSave.Key = null;
                                }
                                if (button==SelectCurrent) {
                                    if (! string.IsNullOrEmpty(stateToSave.Key)) {
                                        SettingManager[_savedItems.FullKey()].SetRaw(stateToSave.Key);
                                    } else {
                                        SettingManager[_nameSetting.FullKey()].SetRaw(string.Empty);
                                    }
                                }
                                SettingManager[_existing.FullKey()].SetRaw(SerializeObject(stateToSave));
                                if (button != SelectCurrent) {
                                    var toSave = GetNodeToSave(m);
                                    if (toSave != null) {
                                        var oldKey = toSave.State.Key;
                                        NodeStore.Save(toSave);
                                        if (toSave.State.Key != oldKey) {
                                            SettingManager[_existing.FullKey()].SetRaw(SerializeObject(toSave.State));
                                        }
                                        _savedItems.Cast<ISavable>()?.Reset();
                                        SettingManager[_savedItems.FullKey()].SetRaw(toSave.State.Key);
                                    }
                                }
                                SettingManager[_description.FullKey()].SetRaw(NodeDescription);
                            }
                        },
                        {
                            _itemButtons.FullKey(), (m,s) => {
                                string button = s.Get<string>();
                                if (button == EditButton) {
                                    var stateToEdit = DeserializeString<PluginState>(_existing.Value<string>(SettingManager));
                                    if (stateToEdit != null) {
                                        var target = stateToEdit.PluginName;
                                        PluginManager[target]?.Cast<T>()?.SetPluginState(stateToEdit);
                                        NavigationManager.NavigateTo(target);
                                    }
                                } else if (button == RenameSelected) {
                                    var toUpdate = NodeStore.GetByKey(_savedItems.Value<string>(SettingManager));
                                    toUpdate.Name = _nameSetting.Value<string>(SettingManager);
                                    NodeStore.Save(toUpdate);
                                    _savedItems.Cast<ISavable>()?.Reset();
                                } else if (button == DeleteButton) {
                                    NodeStore.Delete(_savedItems.Value<string>(SettingManager));
                                    _savedItems.Cast<ISavable>()?.Reset();
                                }
                            }
                        },
                        {
                            _savedItems.FullKey(), (m,s) => {
                                var savedNode=(_savedItems.FindMatchingItem(s.Get<string>()) as NodeItem<T>)?._node;
                                if (savedNode != null) {
                                    SettingManager[_nameSetting.FullKey()].SetRaw(savedNode.Name);
                                    SettingManager[_existing.FullKey()].SetRaw(SerializeObject(savedNode.State));
                                    SettingManager[_pluginSelection.FullKey()].SetRaw(savedNode.State.PluginName);
                                    SettingManager[_description.FullKey()].SetRaw(NodeDescription);
                                }
                            }
                        }
                    };
                }
            }
            return this;
        }
        public IEnumerable<IMenuItem> Items => initialize()._menuItems;

        public string NodeDescription {
            get {
                PluginState state = DeserializeString<PluginState>(_existing.Value<string>(SettingManager));
                string selectedDescription = "(none selected)";
                if (state != null) {
                    var plugin = PluginManager[state.PluginName]?.Cast<T>();
                    if (plugin != null) {
                        var existingState = plugin.GetPluginState();
                        plugin.SetPluginState(state);
                        selectedDescription = plugin.NodeDescription ?? selectedDescription;
                        plugin.SetPluginState(existingState);
                    }
                }
                return $"Named {NodeName}:{_nameSetting.Value<string>(SettingManager)} - {selectedDescription}";
            }
        }

        public IEnumerable<string> KeysToEdit => initialize()._editable;

        public IEnumerable<ISettingDefinition> Definitions => initialize()._settingDefinitions;

        public string Name => GetType().FullName;

        public Dictionary<string, Action<IWorkMonitor, ISetting>> ActionsBySettingKey => initialize()._actionsBySettingKey;

        public PluginState GetPluginState(ISet<string> visited = null)
        {
            return DeserializeString<PluginState>(_existing.Value<string>(SettingManager));
        }

        public void SetPluginState(PluginState state, ISet<string> visited = null)
        {
            SettingManager[_pluginSelection.FullKey()].SetRaw(state.PluginName);
            SettingManager[_existing.FullKey()].SetRaw(SerializeObject(state));
            SettingManager[_description.FullKey()].SetRaw(NodeDescription);
        }

        public void AfterComposition()
        {
            List<IPlugin> plugins = PluginManager.Plugins.Where(p => p is T && p is IHasSaveKey).ToList();
            _pluginSelection.SetPlugins(plugins, SettingManager);
        }
    }
}
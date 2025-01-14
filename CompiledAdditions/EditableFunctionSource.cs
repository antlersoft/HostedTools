using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using com.antlersoft.HostedTools.ConditionBuilder;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Navigation;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;

namespace com.antlersoft.HostedTools.CompiledAdditions {
    [Export(typeof(IFunctionSource))]
    [Export(typeof(IAfterComposition))]
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(EditableFunctionSource))]
    public class EditableFunctionSource : EditOnlyPlugin, IFunctionSource, IAfterComposition, IHasSettingChangeActions, ISettingDefinitionSource
    {
        [Import]
        public IFunctionStore? FunctionStore {get;set;}
        [Import]
        public IWorkMonitorSource? MonitorSource {get;set;}
        [Import]
        public INavigationManager? NavigationManager {get;set;}
        [Import]
        public NamespaceEditor? namespaceEditor {get;set;}

        static ISettingDefinition NamespaceName = new SimpleSettingDefinition("NamespaceName", "EditableFunctionSource", "Namespace Name", "Namespace to add or edit");
        static NamespaceSelectionDefinition NamespaceSelection = new NamespaceSelectionDefinition();
        static ISettingDefinition AddButton = new ButtonsDefinition("AddButton", "EditableFunctionSource", new [] { "Add"});
        static internal Regex NameRegex = new Regex("^[_a-zA-Z][_a-zA-Z0-9]*$");

        Dictionary<string, Action<IWorkMonitor, ISetting>> _actionsBySettingKey;
        private SortedDictionary<string,FunctionNamespace> _namespaces = new SortedDictionary<string, FunctionNamespace>();

        private void SetEditNamespace(string name) {
            namespaceEditor?.SetNamespace(GetNamespaceForName(name));
        }

        internal FunctionNamespace GetNamespaceForName(string name) {
            FunctionNamespace? existing = null;
            _namespaces.TryGetValue(name, out existing);
            FunctionNamespace result = existing ?? new FunctionNamespace(name, FunctionStore, MonitorSource);
            if (existing == null) {
                _namespaces.Add(name, result);
            }
            return result;
        }

        public EditableFunctionSource()
        : base(new MenuItem("DevTools.EditablePluginSource","Compiled Function Namespaces", typeof(EditableFunctionSource).FullName, "DevTools"),
            new[] {NamespaceName.FullKey(), NamespaceSelection.FullKey(), AddButton.FullKey()})
        {
            _actionsBySettingKey = new Dictionary<string, Action<IWorkMonitor, ISetting>> {
                { AddButton.FullKey(), (m,s) => {
                    var n = NamespaceName.Value<string>(SettingManager);
                    
                    if (! NameRegex.IsMatch(n)) {
                        m.Thrown = new ArgumentException($"'{n}' is not a valid namespace name");
                        m.Writer.WriteLine("Bad namespace name");
                        return;
                    }
                    SetEditNamespace(n);
                    NavigationManager?.NavigateTo(typeof(NamespaceEditor).FullName);
                }}
            };
        }
        
        public IEnumerable<IFunctionNamespace> AvailableNamespaces => _namespaces.Values.Select(n => (IFunctionNamespace)n);

        public Dictionary<string, Action<IWorkMonitor, ISetting>> ActionsBySettingKey => _actionsBySettingKey;
        public IEnumerable<ISettingDefinition> Definitions => new ISettingDefinition[] { NamespaceName, NamespaceSelection, AddButton };

        public void AfterComposition()
        {
            foreach (var n in FunctionStore?.GetNamespaces()??new string[0]) {
                _namespaces.Add(n, new FunctionNamespace(n, FunctionStore, MonitorSource));
            }
            NamespaceSelection._container = this;
        }

        class NamespaceSelectionDefinition : SimpleSettingDefinition, IItemSelectionDefinition {
            internal EditableFunctionSource? _container { get; set; }
            internal NamespaceSelectionDefinition()
            : base("NamespaceSelection", "EditableFunctionSource", "Existing namespaces")
            {

            }

            public object FindMatchingItem(string rawText)
            {
                return rawText;
            }

            public IEnumerable<object> GetAllItems()
            {
                return ((IEnumerable<object>?)_container?._namespaces.Keys)??new object[0];
            }

            public string GetRawTextForItem(object item)
            {
                return (string)item;
            }

            public bool IncludeEditButton()
            {
                return true;
            }

            public string NavigateToOnEdit(object item)
            {
                _container?.SetEditNamespace((string)item);
                return typeof(NamespaceEditor).FullName??string.Empty;
            }
        }
    }
}
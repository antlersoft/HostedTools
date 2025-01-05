using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Framework.Model.UI;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.CompiledAdditions {
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(NamespaceEditor))]
    public class NamespaceEditor : SimpleWorker, IHasSettingChangeActions, ISettingDefinitionSource {
        [Import]
        public IFunctionStore? FunctionStore {get; set;}
        [Import]
        public IConditionBuilder? ConditionBuilder {get;set;}

        private FunctionNamespace? _namespace;
        private SavedFunction? _function;
        private Dictionary<string, Action<IWorkMonitor, ISetting>> _actionsBySettingKey;
        private static readonly string Scope = "NamespaceEditor";

        internal static ISettingDefinition NamespaceName = new SimpleSettingDefinition("NamespaceName", Scope, "Namespace");
        internal static ISettingDefinition FunctionName = new SimpleSettingDefinition("FunctionName",Scope,"Function Name");
        internal static ISettingDefinition TestExpression = new SimpleSettingDefinition("TestExpression", Scope,"Test Expression");
        static FunctionSelectionDefinition FunctionSelection = new FunctionSelectionDefinition();
        static ISettingDefinition Usings = new MultiLineSettingDefinition("Usings", Scope, 3, "Using declarations");
        static ISettingDefinition OtherDeclarations = new MultiLineSettingDefinition("OtherDeclarations", Scope, 3, "In-class declarations");
        static ISettingDefinition Body = new MultiLineSettingDefinition("Body", Scope, 5, "Function body");
        static ISettingDefinition AddButton = new ButtonsDefinition("AddButton", Scope, new[] {"Add function"});

        private void SetFieldsFromEditedFunction() {
            if (_function != null) {
                SettingManager[FunctionName.FullKey()].SetRaw(_function.Name);
                SettingManager[Usings.FullKey()].SetRaw(_function.Definition[FunctionNamespace.UsingStatements].AsString);
                SettingManager[OtherDeclarations.FullKey()].SetRaw(_function.Definition[FunctionNamespace.Declarations].AsString);
                SettingManager[Body.FullKey()].SetRaw(_function.Definition[FunctionNamespace.Body].AsString);
            }
        }

        private void SetCurrentFunction(string key) {
            var saved = FunctionStore?.GetByKey(key);
            if (saved != null) {
                _function = saved;
                SetFieldsFromEditedFunction();
            }
        }

        private bool ValidateFunctionName(IWorkMonitor m) {
            if (_namespace==null) {
                m.Writer.WriteLine("Namespace not set!");
                return false;
            }
            string name = FunctionName.Value<string>(SettingManager);
            if (! EditableFunctionSource.NameRegex.IsMatch(name)) {
                m.Writer.WriteLine($"'{name}' is not a valid function name");
                return false;
            }
            if (name!=_function?.Name) {
                if (FunctionSelection._functionList.Any(f => f.Name == name)) {
                    m.Writer.WriteLine($"'{name}' is already a function name in this namespace");
                    return false;
                }
            }
            return true;
        }

        private void SaveEditedFunction(IWorkMonitor m, ISetting s) {
            if (! ValidateFunctionName(m))
                return;
            string name = FunctionName.Value<string>(SettingManager);
            string usings = Usings.Value<string>(SettingManager);
            string declarations = OtherDeclarations.Value<string>(SettingManager);
            string body = Body.Value<string>(SettingManager);

            bool changeRequired = false;
            SavedFunction function = _function ?? new SavedFunction();
            if (_function == null) {
                _function = function;
                function.Namespace = _namespace?.Name;
                function.Name = name;
                changeRequired = true;
            }
            if (name != function.Name) {
                changeRequired=true;
                function.Name = name;
            }
            if (string.IsNullOrWhiteSpace(function.Key)) {
                changeRequired = true;
            }
            if (usings != function.Definition[FunctionNamespace.UsingStatements]?.AsString) {
                changeRequired = true;
                function.Definition[FunctionNamespace.UsingStatements] = new JsonHtValue(usings);
            }
            if (string.IsNullOrWhiteSpace(body)) {
                m.Writer.WriteLine("A function body is required");
                return;
            }
            if (body != function.Definition[FunctionNamespace.Body]?.AsString) {
                changeRequired = true;
                function.Definition[FunctionNamespace.Body] = new JsonHtValue(body);
            }
            if (declarations != function.Definition[FunctionNamespace.Declarations]?.AsString) {
                changeRequired = true;
                function.Definition[FunctionNamespace.Declarations] = new JsonHtValue(declarations);
            }
            if (changeRequired) {
                _namespace?.addOrUpdateFunction(function);
                FunctionSelection.Cast<ISavable>()?.Reset();
            }
        }
        
        internal void SetNamespace(FunctionNamespace name) {
            if (name != _namespace) {
                _namespace = name;
                SettingManager[NamespaceName.FullKey()].SetRaw(name.Name);
                FunctionSelection.Cast<ISavable>()?.Reset();
                if (FunctionSelection._functionList.Count>0) {
                    SetCurrentFunction(FunctionSelection._functionList[0].Key);
                }
            }
        }

        public NamespaceEditor()
        : base(new MenuItem("DevTools.NamespaceEditor", "Edit a namespace", typeof(NamespaceEditor).FullName, "DevTools"),
        new [] { NamespaceName.FullKey(), FunctionSelection.FullKey(), FunctionName.FullKey(), AddButton.FullKey(), Usings.FullKey(), OtherDeclarations.FullKey(), Body.FullKey(), TestExpression.FullKey()})
        {
            NamespaceName.Cast<IAggregator>().InjectImplementation(typeof(IReadOnly), new CanBeReadOnly(true));
            FunctionSelection._container = this;
            _actionsBySettingKey = new Dictionary<string, Action<IWorkMonitor, ISetting>> {
                { AddButton.FullKey(), (m,s) => {
                    if (! ValidateFunctionName(m))
                        return;
                    string name = FunctionName.Value<string>(SettingManager);
                    _function = new SavedFunction();
                    _function.Definition = new JsonHtValue();
                    _function.Namespace = _namespace?.Name;
                    _function.Name = name;
                }},
                {FunctionName.FullKey(), SaveEditedFunction},
                {Body.FullKey(), SaveEditedFunction},
                {OtherDeclarations.FullKey(), SaveEditedFunction},
                {Usings.FullKey(), SaveEditedFunction}
            };
        }

        public Dictionary<string, Action<IWorkMonitor, ISetting>> ActionsBySettingKey => _actionsBySettingKey;

        public IEnumerable<ISettingDefinition> Definitions => new ISettingDefinition[] { NamespaceName, FunctionName, TestExpression, FunctionSelection, Usings, OtherDeclarations, Body, AddButton };

        public override void Perform(IWorkMonitor monitor)
        {
            string test=TestExpression.Value<string>(SettingManager);
            if (string.IsNullOrWhiteSpace(test)) {
                monitor.Writer.WriteLine("Test expression is empty");
                return;
            }
            IHtExpression? expression = ConditionBuilder?.ParseCondition(test);
            if (expression == null) {
                monitor.Writer.WriteLine("Unable to parse expression");
            } else {
                monitor.Writer.WriteLine("Result = "+expression.Evaluate(new JsonHtValue()).AsString);
            }
        }

        class FunctionSelectionDefinition : SimpleSettingDefinition, IItemSelectionDefinition {
            internal NamespaceEditor? _container { get; set; }
            internal FunctionSelectionDefinition()
            : base("FunctionSelection", "NamespaceEditor", "Existing functions", "Existing functions in the namespace")
            {
                _functionList = new List<SavedFunction>();
                _functionsByKey = new Dictionary<string, SavedFunction>();
            }

            private Dictionary<string,SavedFunction> _functionsByKey;
            internal IList<SavedFunction> _functionList;

            public object FindMatchingItem(string rawText)
            {
                SavedFunction result = null;
                _functionsByKey.TryGetValue(rawText, out result);
                return result;
            }

            public IEnumerable<object> GetAllItems()
            {
                _functionList = (((IEnumerable<SavedFunction>?)_container?.FunctionStore?.GetNamespaceFunctions(_container?._namespace?.Name??string.Empty))??Array.Empty<SavedFunction>()).ToList();
                _functionsByKey.Clear();
                foreach (var f in _functionList) {
                    _functionsByKey[f.Key] = f;
                }
                return _functionList;
            }

            public string GetRawTextForItem(object item)
            {
                return ((SavedFunction)item).Key??string.Empty;
            }

            public bool IncludeEditButton()
            {
                return true;
            }

            public string NavigateToOnEdit(object item)
            {
                _container?.SetCurrentFunction(((SavedFunction)item).Key);
                return typeof(NamespaceEditor).FullName??string.Empty;
            }
        }

    }
}
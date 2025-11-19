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
        [Import]
        public EditableFunctionSource? FunctionSource {get;set;}

        private FunctionNamespace? _namespace;
        private SavedFunction? _function;
        private Dictionary<string, Action<IWorkMonitor, ISetting>> _actionsBySettingKey;
        private static readonly string Scope = "NamespaceEditor";

        private static readonly string SaveButtonText = "Save function";
        private static readonly string DeleteButtonText = "Delete function";

        internal static ISettingDefinition NamespaceName = new SimpleSettingDefinition("NamespaceName", Scope, "Namespace");
        internal static ISettingDefinition FunctionName = new SimpleSettingDefinition("FunctionName",Scope,"Function Name");
        internal static ISettingDefinition TestExpression = new SimpleSettingDefinition("TestExpression", Scope,"Test Expression");
        static FunctionSelectionDefinition FunctionSelection = new FunctionSelectionDefinition();
        static ISettingDefinition Usings = new MultiLineSettingDefinition("Usings", Scope, 3, "Using declarations", null, null, null, false);
        static ISettingDefinition OtherDeclarations = new MultiLineSettingDefinition("OtherDeclarations", Scope, 3, "In-class declarations", null, null, null, false);
        static ISettingDefinition Body = new MultiLineSettingDefinition("Body", Scope, 5, "Function body", "Body of function declared IHtValue [functionName](IList<IHtValue> args)", null, null, false);
        static ISettingDefinition Description = new SimpleSettingDefinition("Description", Scope, "Description");
        static ISettingDefinition DeleteButton = new ButtonsDefinition("DeleteButton", Scope, new[] { SaveButtonText, DeleteButtonText });

        private void SetFieldsFromEditedFunction()
        {
            if (_function != null)
            {
                SettingManager[FunctionName.FullKey()].SetRaw(_function.Name);
                SettingManager[Usings.FullKey()].SetRaw(_function.Definition[FunctionNamespace.UsingStatements]?.AsString ?? string.Empty);
                SettingManager[OtherDeclarations.FullKey()].SetRaw(_function.Definition[FunctionNamespace.Declarations]?.AsString ?? string.Empty);
                SettingManager[Body.FullKey()].SetRaw(_function.Definition[FunctionNamespace.Body]?.AsString ?? string.Empty);
                SettingManager[Description.FullKey()].SetRaw(_function.Definition[FunctionNamespace.Description]?.AsString ?? string.Empty);
            }
            else
            {
                SettingManager[FunctionName.FullKey()].SetRaw("(name required)");
                SettingManager[Usings.FullKey()].SetRaw(string.Empty);
                SettingManager[OtherDeclarations.FullKey()].SetRaw(string.Empty);
                SettingManager[Body.FullKey()].SetRaw(string.Empty);
                SettingManager[Description.FullKey()].SetRaw(string.Empty);
            }
            UpdateButtonState();
        }
        
        private void UpdateButtonState()
        {
            if (DeleteButton.Cast<IReadOnly>() is CanBeReadOnly readOnly)
            {
                if (_function != null)
                {

                    readOnly.KeyReadOnly(DeleteButtonText, false);
                    readOnly.KeyReadOnly(SaveButtonText,
                        FunctionName.Value<string>(SettingManager) == _function.Name &&
                        Usings.Value<string>(SettingManager) == _function.Definition[FunctionNamespace.UsingStatements]?.AsString &&
                        OtherDeclarations.Value<string>(SettingManager) == _function.Definition[FunctionNamespace.Declarations]?.AsString &&
                        Body.Value<string>(SettingManager) == _function.Definition[FunctionNamespace.Body]?.AsString &&
                        Description.Value<string>(SettingManager) == _function.Definition[FunctionNamespace.Description]?.AsString
                        );
                }
                else
                {
                    readOnly.KeyReadOnly(SaveButtonText, false);
                    readOnly.KeyReadOnly(DeleteButtonText, true);
                }
            }
        }

        private void SetCurrentFunction(string key) {
            _function = string.IsNullOrEmpty(key) ? null : FunctionStore?.GetByKey(key);
            SetFieldsFromEditedFunction();
        }

        private bool ValidateFunctionName(IWorkMonitor m) {
            if (_namespace==null) {
                string namespaceName = NamespaceName.Value<string>(SettingManager);
                if (string.IsNullOrWhiteSpace(namespaceName) || ! EditableFunctionSource.NameRegex.IsMatch(namespaceName)) {
                    m.Writer.WriteLine("Namespace not set!");
                    return false;
                } else {
                    _namespace = FunctionSource?.GetNamespaceForName(namespaceName);
                    FunctionSelection.GetAllItems();
                }
            }
            string name = FunctionName.Value<string>(SettingManager);
            if (! EditableFunctionSource.NameRegex.IsMatch(name)) {
                m.Writer.WriteLine($"'{name}' is not a valid function name");
                return false;
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
            string description = Description.Value<string>(SettingManager);
            bool changeRequired = false;
            SavedFunction function = _function ?? new SavedFunction();
            if (_function == null)
            {
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
                FunctionSelection.GetAllItems();
                if (FunctionSelection._functionList.Any(sf => sf.Name == name)) {
                    m.Writer.WriteLine("Can't create new function with existing name " + name);
                    return;
                }
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
            if (description != function.Definition[FunctionNamespace.Description]?.AsString) {
                changeRequired = true;
                function.Definition[FunctionNamespace.Description] = new JsonHtValue(description);
            }
            if (changeRequired)
            {
                _namespace?.addOrUpdateFunction(function);
                FunctionSelection.Cast<ISavable>()?.Reset();
            }
            UpdateButtonState();
        }
        
        internal void SetNamespace(FunctionNamespace name) {
            if (name != _namespace) {
                _namespace = name;
                SettingManager[NamespaceName.FullKey()].SetRaw(name.Name);
                ISavable? savable=FunctionSelection.Cast<ISavable>();
                if (savable != null) {
                    savable.Reset();
                    if (FunctionSelection._functionList.Count>0) {
                        SetCurrentFunction(FunctionSelection._functionList[0].Key);
                    }
                } else {
                    var first = FunctionStore?.GetNamespaceFunctions(name.Name).FirstOrDefault();
                    if (first!=null) {
                        SetCurrentFunction(first.Key);
                    }
                }
            }
        }

        public NamespaceEditor()
        : base(new MenuItem("DevTools.NamespaceEditor", "Edit a namespace", typeof(NamespaceEditor).FullName, "DevTools"),
        new [] { NamespaceName.FullKey(), FunctionSelection.FullKey(), FunctionName.FullKey(), DeleteButton.FullKey(), Usings.FullKey(), OtherDeclarations.FullKey(), Body.FullKey(), Description.FullKey(), TestExpression.FullKey()})
        {
            NamespaceName.Cast<IAggregator>().InjectImplementation(typeof(IReadOnly), new CanBeReadOnly(true));
            DeleteButton.Cast<IAggregator>().InjectImplementation(typeof(IReadOnly), new CanBeReadOnly(false));
            FunctionSelection._container = this;
            Action<IWorkMonitor, ISetting> updateButtons = (m, s) => UpdateButtonState();
            _actionsBySettingKey = new Dictionary<string, Action<IWorkMonitor, ISetting>> {
                { DeleteButton.FullKey(), (m,s) =>
                {
                    string key = _function?.Key??string.Empty;
                    if (s.GetRaw()==SaveButtonText)
                    {
                        SaveEditedFunction(m, s);
                        SettingManager[FunctionSelection.FullKey()].SetRaw(key);
                    }
                    else if (key.Length > 0)
                    {
                        _namespace?.DeleteFunction(_function.Key);
                        SettingManager[FunctionSelection.FullKey()].SetRaw("");
                        SetCurrentFunction(string.Empty);
                    }
                    FunctionSelection.Cast<ISavable>()?.Reset();
                }
                },
                { FunctionName.FullKey(), updateButtons},
                { Usings.FullKey(), updateButtons },
                { OtherDeclarations.FullKey(), updateButtons},
                { Body.FullKey(), updateButtons}
            };
        }

        public Dictionary<string, Action<IWorkMonitor, ISetting>> ActionsBySettingKey => _actionsBySettingKey;

        public IEnumerable<ISettingDefinition> Definitions => new ISettingDefinition[] { NamespaceName, FunctionName, TestExpression, FunctionSelection, Usings, OtherDeclarations, Body, Description, DeleteButton };

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
            internal List<SavedFunction> _functionList;

            public object FindMatchingItem(string rawText)
            {
                SavedFunction result = null;
                _functionsByKey.TryGetValue(rawText, out result);
                return result;
            }

            public IEnumerable<object> GetAllItems()
            {
                SavedFunction dummyFunction = new SavedFunction();
                dummyFunction.Name = "(new function)";
                _functionList.Clear();
                _functionList.Add(dummyFunction);
                _functionList.AddRange((((IEnumerable<SavedFunction>?)_container?.FunctionStore?.GetNamespaceFunctions(_container?._namespace?.Name??string.Empty))??Array.Empty<SavedFunction>()).ToList());
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
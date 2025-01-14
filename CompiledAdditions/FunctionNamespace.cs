using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using com.antlersoft.HostedTools.ConditionBuilder;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace com.antlersoft.HostedTools.CompiledAdditions {
    /// <summary>
    /// Represents a dynamically compiled class
    /// </summary>
    public class FunctionNamespace : IFunctionNamespace
    {
        private string _name;
        private IFunctionStore _persistence;
        private IWorkMonitorSource _workMonitorSource;
        private object _lock = new object();
        private Dictionary<string, Func<IList<IHtValue>, IHtValue>>? _namespaceFunctions;
        private MyLoadContext? _unloadableCompiledContext;
        private PortableExecutableReference[] _refFiles;

        internal static readonly string UsingStatements = "UsingStatements";
        internal static readonly string Declarations = "Declarations";
        internal static readonly string Body = "Body";
        internal static readonly string ClassNamespace = "com.antlersoft.HostedTools.UserFunctions";

        private string BuildClassDefinition(IList<SavedFunction> functions) {
            StringBuilder sb = new StringBuilder();

            sb.Append("using System;\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append("using com.antlersoft.HostedTools.Interface;\n");
            sb.Append("using com.antlersoft.HostedTools.ConditionBuilder;\n");
            sb.Append("using com.antlersoft.HostedTools.Serialization;\n");
            foreach (var f in functions) {
                sb.Append(f.Definition[UsingStatements].AsString);
                sb.Append("\n");
            }
            sb.Append("namespace ");
            sb.Append(ClassNamespace);
            sb.Append("{\nclass ");
            sb.Append(Name);
            sb.Append(" {\n");

            foreach (var f in functions) {
                sb.Append(f.Definition[Declarations].AsString);
                sb.Append("\n");
            }
            foreach (var f in functions) {
                sb.Append("public IHtValue ");
                sb.Append(f.Name);
                sb.Append("(IList<IHtValue> args) {\n");
                sb.Append(f.Definition[Body].AsString);
                sb.Append("}\n");
            }

            sb.Append("} }");

            return sb.ToString();
        }

        private void ClearCompiled() {
            lock (_lock) {
                _namespaceFunctions = null;
                _unloadableCompiledContext?.Unload();
                _unloadableCompiledContext = null;
            }
        }

        internal FunctionNamespace(string name, IFunctionStore persistence, IWorkMonitorSource workMonitorSource) {
            _name = name;
            _persistence = persistence;
            _workMonitorSource = workMonitorSource;
            var refs = AppDomain.CurrentDomain.GetAssemblies();
            _refFiles = refs.Where(a => !a.IsDynamic).Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray();
        }

        internal void addOrUpdateFunction(SavedFunction sf) {
            _persistence.Save(sf);
            ClearCompiled();
        }

        internal void DeleteFunction(string key) {
            _persistence.Delete(key);
            ClearCompiled();
        }

        public string Name => _name;

        public IDictionary<string, Func<IList<IHtExpression>, IGroupExpression>> AddedGroupFunctions  => new Dictionary<string, Func<IList<IHtExpression>, IGroupExpression>>();
        public IDictionary<string, Func<IList<IHtValue>, IHtValue>> AddedFunction {
            get {
                lock (_lock) {
                    var monitor = _workMonitorSource.GetMonitor();
                    if (_namespaceFunctions == null) {
                        _namespaceFunctions = new Dictionary<string, Func<IList<IHtValue>, IHtValue>>();
                        var functions = _persistence.GetNamespaceFunctions(Name).ToList();
                        var code = BuildClassDefinition(functions);
                        var tree = CSharpSyntaxTree.ParseText(code);
                        foreach (var d in tree.GetDiagnostics()) {
                            monitor.Writer.WriteLine("Diagnostic: "+d.ToString());
                        }
                        if (tree.GetDiagnostics().Any(d=>d.Severity==DiagnosticSeverity.Error)) {
                            throw new Exception("Bad parse of:\n"+code);
                        }

                        string assemblyName = Guid.NewGuid().ToString();
                        var compilation = CSharpCompilation.Create(
                            assemblyName,
                            new[] { tree },
                            _refFiles,
                            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                        );
                        Assembly asm;
                        using (var ms = new MemoryStream())
                        {
                            var compilationResult = compilation.Emit(ms);
                            if (! compilationResult.Success) {
                                foreach (var d in compilationResult.Diagnostics) {
                                    monitor.Writer.WriteLine("Diagnostic: "+d.ToString());
                                }
                                throw new Exception("Bad compilation");
                            }
                            ms.Seek(0, SeekOrigin.Begin);
                            asm = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(ms);
                        }
                        var t = asm.GetType("com.antlersoft.HostedTools.UserFunctions."+Name);
                        var o = System.Activator.CreateInstance(t);
                        Type[] argTypes = new [] { typeof(IList<IHtValue>) };
                        foreach (var f in functions) {
                            var m = t.GetMethod(f.Name, argTypes);
                            if (m != null) {
                                _namespaceFunctions[f.Name] = (valueArgs) => (IHtValue)m.Invoke(o, new object[] { valueArgs });
                            }
                        }
                    }
                    return _namespaceFunctions;
                }
            }
        }

        class MyLoadContext : AssemblyLoadContext {
            Assembly? _generatedAssembly;
            internal MyLoadContext(string name)
            : base(name, true) {

            }

            protected override Assembly? Load(AssemblyName assemblyName) {
                if (assemblyName.Name == Name) {
                    return _generatedAssembly;
                }
                return null;
            }
        }
    }

}
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Microsoft.CodeAnalysis;
using System.Formats.Asn1;

namespace com.antlersoft.HostedTools.CompiledAdditions {

// [Export(typeof(ISettingDefinitionSource))]
public class FunctionEditor : SimpleWorker
{
    public FunctionEditor()
    : base(new MenuItem("DevTools.FunctionEditor", "Function Editor", typeof(FunctionEditor).FullName, "DevTools"), new string[0])
    {
    }
    public override void Perform(IWorkMonitor monitor) {
        var refs = AppDomain.CurrentDomain.GetAssemblies();
        var refFiles = refs.Where(a => !a.IsDynamic).Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray();

        string code = @"
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.UserFunctions {
class ExpressionClass : IHtExpression {
    public IHtValue Evaluate(IHtValue arg) {
        return new JsonHtValue(arg.AsDouble * 4.0);
    }
}
    }
        ";
        var tree = CSharpSyntaxTree.ParseText(code);
        foreach (var d in tree.GetDiagnostics()) {
            monitor.Writer.WriteLine("Diagnostic: "+d.ToString());
        }

        string assemblyName = Guid.NewGuid().ToString();
        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { tree },
            refFiles,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        System.Reflection.Assembly asm;
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
        var t = asm.GetType("com.antlersoft.HostedTools.UserFunctions.ExpressionClass");
        var o = System.Activator.CreateInstance(t);
        var method = t.GetMethod("Evaluate");
        var result = method.Invoke(o, new[] {new JsonHtValue(1.0)}) as IHtValue;
        monitor.Writer.WriteLine($"Result = {result.AsString}");
    }
}

}

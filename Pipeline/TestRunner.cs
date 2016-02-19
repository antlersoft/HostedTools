using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof (ISettingDefinitionSource))]
    public class TestRunner : SimpleWorker, ISettingDefinitionSource
    {
        private static ISettingDefinition MethodName = new SimpleSettingDefinition("MethodName", "TestRunner",
            "Method name");

        private static ISettingDefinition ClassName = new SimpleSettingDefinition("ClassName", "TestRunner",
            "Assembly-qualified class name");

        private static ISettingDefinition DllPath = new PathSettingDefinition("DllPath", "TestRunner", "Assembly path",
            false, false, "Assembly|*.exe;*.dll");

        public TestRunner()
            : base(
                new MenuItem("DevTools.TestRunner", "Test runner", typeof (TestRunner).FullName, "DevTools"),
                new [] { MethodName.FullKey(), ClassName.FullKey(), DllPath.FullKey()}
    )

{ }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new [] {MethodName, ClassName, DllPath}; }
        }

        static readonly Type[] EmptyTypes = new Type[0];
        static readonly object[] EmptyArgs = new object[0];

        public override void Perform(IWorkMonitor monitor)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(DllPath.Value<string>(SettingManager));
                if (assembly == null)
                {
                    monitor.Writer.WriteLine("Assembly not loaded-- already loaded?");
                }
                else
                {
                    monitor.Writer.WriteLine("Assembly loaded");
                    foreach (Type ti in assembly.GetExportedTypes())
                    {
                        monitor.Writer.WriteLine(ti.AssemblyQualifiedName);
                    }
                }
            }
            catch (Exception ex)
            {
                monitor.Writer.Write(ex.ToString());
                monitor.Writer.WriteLine("Not fatal");
            }
            Type t = Type.GetType(ClassName.Value<string>(SettingManager));
            if (t == null && assembly != null)
            {
                t = assembly.GetType(ClassName.Value<string>(SettingManager), true);
            }
            if (t == null)
            {
                throw new Exception("Type " + ClassName.Value<string>(SettingManager)+" not found");
            }
            var mi = t.GetMethod(MethodName.Value<string>(SettingManager), EmptyTypes);
            if (mi == null)
            {
                throw new Exception("Method " + MethodName.Value<string>(SettingManager) + " not found");
            }
            mi.Invoke(t.GetConstructor(EmptyTypes).Invoke(EmptyArgs), EmptyArgs);
        }
    }
}

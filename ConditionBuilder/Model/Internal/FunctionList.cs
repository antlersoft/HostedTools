using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    public class FunctionList : SimpleWorker
    {
        [ImportMany]
        public List<IFunctionSource> sources;

        public FunctionList()
        : base(new MenuItem("Common.FunctionList", "Function List", typeof(FunctionList).FullName, "Common"), new string[0])
        { }

        private void WriteNamespace<T>(IFunctionNamespace ns, IDictionary<string,T> functionDictionary, IWorkMonitor monitor) where T : IHostedObject
        {
            if (ns.Name.Length > 0) {
                monitor.Writer.WriteLine($"    Namespace @{ns.Name}");
            }
            else
            {
                monitor.Writer.WriteLine("    Default Namespace");
            }
            foreach (var k in functionDictionary.Keys.OrderBy(k => k))
            {
                T f = functionDictionary[k];
                monitor.Writer.WriteLine($"        {k} {(f.Cast<IFunctionInfo>() == null ? String.Empty : " - " + f.Cast<IFunctionInfo>().Description)}");
            }    
        }
        
        public override void Perform(IWorkMonitor monitor)
        {
            monitor.Writer.WriteLine("Regular functions:");
            foreach (var ns in sources.SelectMany(s => s.AvailableNamespaces).OrderBy(n => n.Name))
            {
                WriteNamespace(ns, ns.AddedFunctions, monitor);
            }
            monitor.Writer.WriteLine("Group functions or function-likes:");
            foreach (var ns in sources.SelectMany(s => s.AvailableNamespaces).OrderBy(n => n.Name))
            {
                WriteNamespace(ns, ns.AddedGroupFunctions, monitor);
            }
        }
    }
}
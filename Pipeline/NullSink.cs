using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ILeafNode))]
    [Export(typeof(IPlugin))]
    public class NullSink : HostedObjectBase, IHtValueLeaf, IHtValueSink, IPlugin
    {
        public void ReceiveRows(IEnumerable<IHtValue> rows, IWorkMonitor monitor)
        {
			var cancelable = monitor.Cast<ICancelableMonitor>();
            foreach (var v in rows)
            {
                if (cancelable!=null && cancelable.IsCanceled)
                {
                    break;
                }
            }
        }

        public IHtValueSink GetHtValueSink(PluginState state)
        {
            return this;
        }

        public string NodeDescription
        {
            get { return "Do nothing"; }
        }
        public PluginState GetPluginState()
        {
            PluginState result = new PluginState();
            result.Description = NodeDescription;
            result.PluginName = Name;
            result.NestedValues = new Dictionary<string, PluginState>();
            result.SettingValues = new Dictionary<string, string>();

            return result;
        }

        public string Name
        {
            get { return GetType().FullName; }
        }
    }
}

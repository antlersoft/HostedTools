using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IPlugin))]
    [Export(typeof(IStemNode))]
    public class NullTransform : IPlugin, IHtValueStem, IHtValueTransform
    {
        public string Name
        {
            get { return GetType().FullName; }
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
        {
            return input;
        }

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return this;
        }

        public PluginState GetPluginState()
        {
            PluginState result = new PluginState();
            result.PluginName = Name;
            result.NestedValues = new Dictionary<string, PluginState>();
            result.SettingValues = new Dictionary<string, string>();

            return result;
        }

        public string NodeDescription
        {
            get { return "Does nothing"; }
        }
    }
}

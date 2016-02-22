using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IPlugin))]
    [Export(typeof(IHtValueTransform))]
    public class NullTransform : IPlugin, IHtValueTransform
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

        public string TransformDescription
        {
            get { return "Does nothing"; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IPlugin))]
    [Export(typeof(IHtValueTransform))]
    public class DistinctTransform : HostedObjectBase, IPlugin, IHtValueTransform
    {
        [Import] public IJsonFactory JsonFactory { get; set; }

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
        {
            HashSet<string> serialized = new HashSet<string>();
            JsonSerializerSettings settings = JsonFactory.GetSettings();
			ICancelableMonitor cancelable = monitor.Cast<ICancelableMonitor>();
            foreach (var row in input)
            {
                if (cancelable != null && cancelable.IsCanceled)
                {
                    yield break;
                }
                string str = JsonConvert.SerializeObject(row, settings);
                if (serialized.Add(str))
                {
                    yield return row;
                }
            }
        }

        public string TransformDescription
        {
            get { return "distinct"; }
        }

        public string Name
        {
            get { return GetType().FullName; }
        }
    }
}

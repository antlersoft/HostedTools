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
    [Export(typeof(IHtValueSink))]
    public class NullSink : EditOnlyPlugin, IHtValueSink
    {
        public NullSink()
            : base(new MenuItem[0], new string[0])
        { }
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

        public string SinkDescription
        {
            get { return "Do nothing"; }
        }
    }
}

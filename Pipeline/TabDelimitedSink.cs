using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Framework.Model.Setting.Internal;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IHtValueSink))]
    public class TabDelimitedSink : EditingSettingDeclarer, IHtValueSink
    {
        [Import] public ISettingManager SettingManager;
        [Import] public IJsonFactory JsonFactory;
        public TabDelimitedSink()
            : base(
                new []
                {
                    new MenuItem("DevTools.Pipeline.Output.TabDelimited", "Tab-delimited output file",
                        typeof (TabDelimitedSink).FullName, "DevTools.Pipeline.Output")
                },
                new[]
                {
                    new PathSettingDefinition("TabDelimitedOutput", "Pipeline", "Tab-delimited output file", true, false,
                        "Tab-delimited text|*.txt"),
                    new SimpleSettingDefinition("ColumnTailer", "Pipeline", "Column Names at End of File", "If not set, no column names are written", typeof(bool), "false", false, 1)

                })
        {
        }

        public void ReceiveRows(IEnumerable<IHtValue> rows, IWorkMonitor monitor)
        {
            List<string> columns = new List<string>();
            JsonSerializer serializer = JsonFactory.GetSerializer(false);
            var writeTail = SettingManager["Pipeline.ColumnTailer"].Get<bool>();
			var cancelable = monitor.Cast<ICancelableMonitor>();

            using (StreamWriter writer = new StreamWriter(SettingManager["Pipeline.TabDelimitedOutput"].Get<string>()))
            {
                bool first;
                foreach (var row in rows)
                {
                    if (cancelable!=null && cancelable.IsCanceled)
                    {
                        break;
                    }
                    first = true;
                    foreach (var name in columns)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            writer.Write("\t");
                        }
                        var val = row[name];
                        if (! val.IsEmpty)
                        {
                            writer.Write(GridWorker.ValueToSimpleObject(serializer, val));
                        }
                    }
                    foreach (var kvp in row.AsDictionaryElements)
                    {
                        if (!columns.Contains(kvp.Key) && ! kvp.Value.IsEmpty)
                        {
                            columns.Add(kvp.Key);
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                writer.Write("\t");
                            }
                            writer.Write(GridWorker.ValueToSimpleObject(serializer, kvp.Value));
                        }
                    }
                    writer.WriteLine();
                }
                if (writeTail)
                {
                    first = true;
                    foreach (var v in columns)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            writer.Write("\t");
                        }
                        writer.Write(v);
                    }
                    if (! first)
                    {
                        writer.WriteLine();
                    }
                }
            }
        }

        public string SinkDescription
        {
            get { return "Write to tab-delimited "+SettingManager["Pipeline.TabDelimitedOutput"].Get<string>(); }
        }
    }
}

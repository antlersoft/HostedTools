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
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Framework.Model.Setting.Internal;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(ILeafNode))]
    public class TabDelimitedSink : AbstractPipelineNode, IHtValueLeaf, ISettingDefinitionSource
    {
        [Import] public IJsonFactory JsonFactory;

        static ISettingDefinition TabDelimitedOutput = new PathSettingDefinition("TabDelimitedOutput", "Pipeline", "Tab-delimited output file", true, false,
                        "Tab-delimited text|*.txt");
        static ISettingDefinition ColumnHeader = new SimpleSettingDefinition("ColumnTailer", "Pipeline", "Column Names as file header", "If not set, no column names are written", typeof(bool), "false", false, 1);

        public TabDelimitedSink()
            : base(
                new []
                {
                    new MenuItem("DevTools.Pipeline.Output.TabDelimited", "Tab-delimited output file",
                        typeof (TabDelimitedSink).FullName, "DevTools.Pipeline.Output")
                },
                new[]
                {
                    TabDelimitedOutput.FullKey(),
                    ColumnHeader.FullKey()
                })
        {
        }

        class Sink : HostedObjectBase, IHtValueSink {
            private readonly bool writeTail;
            private readonly string destFile;
            private readonly JsonSerializer serializer;

            internal Sink(string outputPath, bool wt, JsonSerializer ser) {
                destFile = outputPath;
                writeTail = wt;
                serializer = ser;
            }

            public void ReceiveRows(IEnumerable<IHtValue> rows, IWorkMonitor monitor)
            {
                List<string> columns = new List<string>();
                var outputPath = writeTail ? Path.GetTempFileName() : destFile;
                var cancelable = monitor.Cast<ICancelableMonitor>();

                using (StreamWriter writer = new StreamWriter(outputPath))
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
                            if (val!=null && ! val.IsEmpty)
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
                }
                if (writeTail)
                {
                    using (StreamWriter writer = new StreamWriter(destFile))
                    {
                        bool first = true;
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
                        using (StreamReader sr = new StreamReader(outputPath))
                        {
                            for (string line=sr.ReadLine(); line!=null; line=sr.ReadLine())
                            {
                                writer.WriteLine(line);
                            }
                        }
                        File.Delete(outputPath);
                    }
                }
            }
        }

        public IHtValueSink GetHtValueSink(PluginState state)
        {
            var writeTail = (bool)Convert.ChangeType(state.SettingValues[ColumnHeader.FullKey()], typeof(bool));
            var destFile = state.SettingValues[TabDelimitedOutput.FullKey()];
            return new Sink(destFile, writeTail, JsonFactory.GetSerializer(false));
        }

        public override string NodeDescription
        {
            get { return "Write to tab-delimited "+SettingManager["Pipeline.TabDelimitedOutput"].Get<string>(); }
        }

        public IEnumerable<ISettingDefinition> Definitions => new ISettingDefinition[] { TabDelimitedOutput, ColumnHeader};
    }
}

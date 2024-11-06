using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using CsvHelper;
using System.Globalization;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IRootNode))]
    [Export(typeof(ISettingDefinitionSource))]
    public class CsvSource : AbstractPipelineNode, IHtValueRoot, ISettingDefinitionSource
    {
        static ISettingDefinition CsvInput = new PathSettingDefinition("CsvInput", "Pipeline", "CSV input file", false, false,
                        "CSV file|*.csv");
        static ISettingDefinition HeaderRow = new SimpleSettingDefinition("UseFirstRowAsHeader", "Pipeline", "Use first row as header", "Use the first row of the file as the header", typeof(bool), "true", false);

        public CsvSource()
            : base(
                    new MenuItem("DevTools.Pipeline.Input.Csv", "CSV input file",
                        typeof (CsvSource).FullName, "DevTools.Pipeline.Input")
                ,
                new[]
                {
                    CsvInput.FullKey(),
                    HeaderRow.FullKey()
                })
        {
        }

        public override string NodeDescription => "Read a standard CSV format file (as exported by Excel, etc.) "+SettingManager["Pipeline.CsvInput"].Get<string>();

        public IEnumerable<ISettingDefinition> Definitions => new[] { CsvInput, HeaderRow};

        class Source : HostedObjectBase, IHtValueSource, IDisposable
        {
            private StreamReader reader;
            private CsvReader csv;
            private bool _useHeader;

            internal Source(string file, bool useHeader) {
                reader = new StreamReader(file);
                csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                _useHeader = useHeader;
            }
            private static string columnKey(string[] headers, int index)
            {
                if (headers != null && index < headers.Length)
                {
                    return headers[index];
                }
                return new string((char)('A' + index), 1);
            }

            public void Dispose()
            {
                csv.Dispose();
                reader.Dispose();
            }

            public IEnumerable<IHtValue> GetRows(IWorkMonitor monitor)
            {
                string[] columnHeaders;
                ICancelableMonitor cancelable = monitor.Cast<ICancelableMonitor>();
                if (_useHeader) {
                    csv.Read();
                    csv.ReadHeader();
                    columnHeaders = csv.HeaderRecord;
                } else {
                    columnHeaders = new string[0];
                }
                while (csv.Read() && (cancelable == null || ! cancelable.IsCanceled))
                {
                    var row = new JsonHtValue();
                    for (int i = 0; i<csv.ColumnCount; i++) {
                        row[columnKey(columnHeaders, i)] = new JsonHtValue(csv.GetField(i));
                    }
                    yield return row;
                }
            }
        }

        public IHtValueSource GetHtValueSource(PluginState state)
        {
            return new Source(state.SettingValues["Pipeline.CsvInput"],
                new JsonHtValue(state.SettingValues["Pipeline.UseFirstRowAsHeader"]).AsBool);
        }
    }
}
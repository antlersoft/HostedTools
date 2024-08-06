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

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IHtValueSource))]
    public class CsvSource : EditingSettingDeclarer, IHtValueSource
    {
        [Import] public ISettingManager SettingManager;
        public CsvSource()
            : base(
                new []
                {
                    new MenuItem("DevTools.Pipeline.Input.Csv", "CSV input file",
                        typeof (CsvSource).FullName, "DevTools.Pipeline.Input")
                },
                new[]
                {
                    new PathSettingDefinition("CsvInput", "Pipeline", "CSV input file", false, false,
                        "CSV file|*.csv"),
                    new SimpleSettingDefinition("UseFirstRowAsHeader", "Pipeline", "Use first row as header", "Use the first row of the file as the header", typeof(bool), "true", false)
                })
        {
        }

        public string SourceDescription => "Read a standard CSV format file (as exported by Excel, etc.) "+SettingManager["Pipeline.CsvInput"].Get<string>();

        private static string columnKey(string[] headers, int index)
        {
            if (headers != null && index < headers.Length)
            {
                return headers[index];
            }
            return new string((char)('A' + index), 1);
        }

        public IEnumerable<IHtValue> GetRows()
        {
            string file = SettingManager["Pipeline.CsvInput"].Get<string>();
            bool useHeader = SettingManager["Pipeline.UseFirstRowAsHeader"].Get<bool>();
            string[] columnHeaders = null;
            using (var reader = new StreamReader(file))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                if (useHeader) {
                    csv.ReadHeader();
                    columnHeaders = csv.HeaderRecord;
                }
                while (csv.Read())
                {
                    var row = new JsonHtValue();
                    for (int i = 0; i<csv.ColumnCount; i++) {
                        row[columnKey(columnHeaders, i)] = new JsonHtValue(csv.GetField(i));
                    }
                    yield return row;
                }
            }
        }
    }
}
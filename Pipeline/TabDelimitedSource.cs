using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.ConditionBuilder.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IHtValueSource))]
    public class TabDelimitedSource : EditingSettingDeclarer, IHtValueSource
    {
        [Import] public ISettingManager SettingManager;
        public TabDelimitedSource()
            : base(
                new []
                {
                    new MenuItem("DevTools.Pipeline.Input.TabDelimited", "Tab-delimited input file",
                        typeof (TabDelimitedSource).FullName, "DevTools.Pipeline.Input")
                },
                new[]
                {
                    new PathSettingDefinition("TabDelimitedInput", "Pipeline", "Tab-delimited input file", false, false,
                        "Tab-delimited text|*.txt"),
                    new MultiLineSettingDefinition("ColumnsToRowExpression", "Pipeline", 8, "Projection expression", "Convert from object with rownum, c[] array fields to output object"), 
                    new SimpleSettingDefinition("SkipInitialLines", "Pipeline", "Number of leading rows to skip", "Skip this many rows into the file before starting", typeof(int), "0", false)
                })
        {
        }

        public IEnumerable<IHtValue> GetRows()
        {
            string file = SettingManager["Pipeline.TabDelimitedInput"].Get<string>();
            string exprString = SettingManager["Pipeline.ColumnsToRowExpression"].Get<string>();
            int rowsToSkip = SettingManager["Pipeline.SkipInitialLines"].Get<int>();
            IHtExpression expr=null;
            if (! String.IsNullOrWhiteSpace(exprString))
            {
                expr = new com.antlersoft.HostedTools.ConditionBuilder.Model.ConditionBuilder().ParseCondition(exprString);
            }
            using (var sr = new StreamReader(file))
            {
                int rowNum = 0;
                for (string line=sr.ReadLine(); line!=null; line=sr.ReadLine())
                {
                    if (rowNum++ < rowsToSkip)
                    {
                        continue;
                    }
                    var columns = new JsonHtValue();
                    int columnNum = 0;
                    foreach (var col in line.Split('\t'))
                    {
                        columns[columnNum++] = new JsonHtValue(col);
                    }
                    var row = new JsonHtValue();
                    row["rownum"] = new JsonHtValue(rowNum - rowsToSkip);
                    row["c"] = columns;
                    if (expr == null)
                    {
                        yield return row;
                    }
                    else
                    {
                        yield return expr.Evaluate(row);
                    }
                }
            }
        }

        public string SourceDescription
        {
            get { return "Read tab-delimited lines from "+SettingManager["Pipeline.TabDelimitedInput"].Get<string>(); }
        }
    }
}

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
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IRootNode))]
    [Export(typeof(ISettingDefinitionSource))]
    public class TabDelimitedSource : AbstractPipelineNode, IHtValueRoot, ISettingDefinitionSource
    {
        static ISettingDefinition TabDelimitedInput = new PathSettingDefinition("TabDelimitedInput", "Pipeline", "Tab-delimited input file", false, false,
                        "Tab-delimited text|*.txt");
        static ISettingDefinition ColumnsToRowExpression = new MultiLineSettingDefinition("ColumnsToRowExpression", "Pipeline", 8, "Projection expression", "Convert from object with rownum, c[] array fields to output object");
        static ISettingDefinition SkipInitialLines = new SimpleSettingDefinition("SkipInitialLines", "Pipeline", "Number of leading rows to skip", "Skip this many rows into the file before starting", typeof(int), "0", false);
        public TabDelimitedSource()
            : base(
                new []
                {
                    new MenuItem("DevTools.Pipeline.Input.TabDelimited", "Tab-delimited input file",
                        typeof (TabDelimitedSource).FullName, "DevTools.Pipeline.Input")
                },
                new[]
                {
                    TabDelimitedInput.FullKey(),
                    ColumnsToRowExpression.FullKey(),
                    SkipInitialLines.FullKey()
                })
        {
        }

        class Source : HostedObjectBase, IHtValueSource {
            private readonly string _filePath;
            private readonly int _rowsToSkip;
            private readonly string _expr;

            internal Source(string filePath, int rowsToSkip, string expr)
            {
                _filePath = filePath;
                _rowsToSkip = rowsToSkip;
                _expr = expr;
            }

            public IEnumerable<IHtValue> GetRows(IWorkMonitor monitor)
            {
                IHtExpression expr=null;
                if (! String.IsNullOrWhiteSpace(_expr))
                {
                    expr = new com.antlersoft.HostedTools.ConditionBuilder.Model.ConditionBuilder().ParseCondition(_expr);
                }
                using (var sr = new StreamReader(_filePath))
                {
                    int rowNum = 0;
                    for (string line=sr.ReadLine(); line!=null; line=sr.ReadLine())
                    {
                        if (rowNum++ < _rowsToSkip)
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
                        row["rownum"] = new JsonHtValue(rowNum - _rowsToSkip);
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
        }

        public IHtValueSource GetHtValueSource(PluginState state)
        {
            return new Source(state.SettingValues[TabDelimitedInput.FullKey()],
                (int)Convert.ChangeType(state.SettingValues[SkipInitialLines.FullKey()], typeof(int)),
                state.SettingValues[ColumnsToRowExpression.FullKey()]);
        }

        public override string NodeDescription
        {
            get { return "Read tab-delimited lines from "+SettingManager["Pipeline.TabDelimitedInput"].Get<string>(); }
        }

        public IEnumerable<ISettingDefinition> Definitions => new [] { TabDelimitedInput, ColumnsToRowExpression, SkipInitialLines};
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IStemNode))]
    class RowRangeTransform : AbstractPipelineNode, IHtValueStem, ISettingDefinitionSource
    {
        static ISettingDefinition StartRow = new SimpleSettingDefinition("StartRow", "Pipeline.Transform", "Start row", "Number of rows to skip at start of sequence", typeof(int), "0");
        static ISettingDefinition NumberOfRows = new SimpleSettingDefinition("NumberOfRows", "Pipeline.Transform", "Number of rows", "Number of rows to return before stopping", typeof(int), "1");

        public RowRangeTransform()
            : base(new MenuItem("DevTools.Pipeline.Transform.RowRange", "Row range", typeof(RowRangeTransform).FullName, "DevTools.Pipeline.Transform"),
                new [] {StartRow.FullKey(), NumberOfRows.FullKey()})
        {}

        public IEnumerable<ISettingDefinition> Definitions => new [] {StartRow, NumberOfRows};

        public override string NodeDescription => $"{NumberOfRows.Value<int>(SettingManager)} rows from row {StartRow.Value<int>(SettingManager)}";

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return new Transform((int)Convert.ChangeType(state.SettingValues[StartRow.FullKey()], typeof(int)),
                (int)Convert.ChangeType(state.SettingValues[NumberOfRows.FullKey()], typeof(int)));
        }

        class Transform : HostedObjectBase, IHtValueTransform {
            private readonly int _startRow;
            private readonly int _numberOfRows;

            internal Transform(int startRow, int numberOfRows) {
                _startRow = startRow;
                _numberOfRows = numberOfRows;
            }
            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> rows, IWorkMonitor monitor)
            {
                int offset = 0;
                foreach (var row in rows)
                {
                    if (offset >= _startRow)
                    {
                        yield return row;
                    }
                    if (++offset >= _startRow + _numberOfRows)
                    {
                        yield break;
                    }
                }
            }
        }
    }
}
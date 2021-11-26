using System.Collections.Generic;
using System.ComponentModel.Composition;

using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IHtValueTransform))]
    class RowRangeTransform : EditOnlyPlugin, IHtValueTransform, ISettingDefinitionSource
    {
        static ISettingDefinition StartRow = new SimpleSettingDefinition("StartRow", "Pipeline.Transform", "Start row", "Number of rows to skip at start of sequence", typeof(int), "0");
        static ISettingDefinition NumberOfRows = new SimpleSettingDefinition("NumberOfRows", "Pipeline.Transform", "Number of rows", "Number of rows to return before stopping", typeof(int), "1");

        public RowRangeTransform()
            : base(new MenuItem("DevTools.Pipeline.Transform.RowRange", "Row range", typeof(RowRangeTransform).FullName, "DevTools.Pipeline.Transform"),
                new [] {StartRow.FullKey(), NumberOfRows.FullKey()})
        {}

        public IEnumerable<ISettingDefinition> Definitions => new [] {StartRow, NumberOfRows};

        public string TransformDescription => $"{NumberOfRows.Value<int>(SettingManager)} rows from row {StartRow.Value<int>(SettingManager)}";

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> rows, IWorkMonitor monitor)
        {
            var startRow = StartRow.Value<int>(SettingManager);
            var numberOfRows = NumberOfRows.Value<int>(SettingManager);
            int offset = 0;
            foreach (var row in rows)
            {
                if (offset >= startRow)
                {
                    yield return row;
                }
                if (++offset >= startRow + numberOfRows)
                {
                    yield break;
                }
            }
        }
    }
}
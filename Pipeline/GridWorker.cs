using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.UI;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.Pipeline
{
    public abstract class GridWorker : SimpleWorker, IOutputPaneList
    {
        private static readonly IOutputPaneList PaneList = new OutputPaneList(EPaneListOrientation.Vertical,
                                                                              new[]
                                                                                  {
                                                                                      new OutputPaneSpecifier(
                                                                                  EOutputPaneType.Text, null, 1),
                                                                                      new OutputPaneSpecifier(
                                                                                  EOutputPaneType.Grid, null, 3)
                                                                                  });

        private JsonSerializer _serializer;

        [Import]
        public IJsonFactory JsonFactory;

        public JsonSerializer Serializer
        {
            get
            {
                if (_serializer == null)
                {
                    _serializer = JsonFactory.GetSerializer(true);
                }
                return _serializer;
            }
        }

        protected GridWorker(IEnumerable<IMenuItem> menuEntries, IEnumerable<string> keys)
            : base(menuEntries, keys)
        {

        }

        protected GridWorker(IMenuItem menuItem, IEnumerable<string> keys)
            : base(menuItem, keys)
        {

        }

        public static void ClearGrid(IWorkMonitor monitor)
        {
            var hasOutput = monitor.Cast<IHasOutputPanes>();
            if (hasOutput != null)
            {
                var grid = hasOutput.FindGridOutput();
                if (grid != null)
                {
                    grid.Clear();
                }
            }
        }

        internal static object ValueToSimpleObject(JsonSerializer serializer, IHtValue val)
        {
            object result;
            if (val == null)
            {
                result = string.Empty;
            }
            else if (val.IsBool)
            {
                result = val.AsBool;
            }
            else if (val.IsDouble)
            {
                result = val.AsDouble;
            }
      else if (val.IsLong)
      {
        result = val.AsLong;
      }
      else if (val.IsEmpty)
            {
                result = string.Empty;
            }
            else if (val.IsString)
            {
                result = val.AsString;
            }
            else
            {
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, val);
                    result = writer.ToString();
                }
            }
            return result;
        }
     
        public void WriteRecord(IGridOutput grid, IHtValue record)
        {
            var kvpList = record.IsDictionary ? record.AsDictionaryElements.OrderBy(k => k.Key).ToList() : new List<KeyValuePair<string, IHtValue>>() {new KeyValuePair<string, IHtValue>("{no columns}", record)};
            Dictionary<string, object> row = new Dictionary<string, object>(kvpList.Count);
            foreach (var kvp in kvpList)
            {
                row.Add(kvp.Key, ValueToSimpleObject(Serializer, kvp.Value));
            }
            grid.AddRow(row);
        }

        public void WriteRecord(IWorkMonitor monitor, IHtValue val)
        {
            var hasOutput = monitor.Cast<IHasOutputPanes>();
            IGridOutput grid = null;
            if (hasOutput != null)
            {
                grid = hasOutput.FindGridOutput();
            }
            if (grid == null)
            {
                monitor.Writer.WriteLine(String.Join(",",
                    val.AsDictionaryElements.Select(kvp => ValueToSimpleObject(Serializer, kvp.Value).ToString())));
            }
            else
            {
                WriteRecord(grid, val);
            }
        }

        public void WriteRecords(IGridOutput grid, IEnumerable<IHtValue> values)
        {
            foreach (var v in values)
            {
                WriteRecord(grid, v);
            }
        }

        public void WriteRecords(IWorkMonitor monitor, IEnumerable<IHtValue> values)
        {
			ICancelableMonitor cancelable = monitor.Cast<ICancelableMonitor>();
            foreach (var v in values)
            {
                if (cancelable!=null && cancelable.IsCanceled)
                {
                    break;
                }
                WriteRecord(monitor, v);
            }
            monitor.Writer.WriteLine("Canceled");
        }

        public virtual EPaneListOrientation Orientation
        {
            get { return PaneList.Orientation; }
        }

        public virtual IList<IOutputPaneSpecifier> Panes
        {
            get { return PaneList.Panes; }
        }
    }
}

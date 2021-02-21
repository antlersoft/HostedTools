using com.antlersoft.HostedTools.Framework.Interface.UI;
using System;
using System.Collections.Generic;
using System.Text;
using Gtk;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class GridOutput : IGridOutput
    {
        ListStore _model;
        public void AddRow(Dictionary<string, object> row)
        {
            if (_model == null)
            {
                Type[] types = new Type[row.Count];
                for (int i = 0; i<row.Count; i++)
                {
                    types[i] = typeof(String);
                }
                _model = new ListStore(types);
            }
            var values = new object[row.Count];
            int j = 0;
            foreach (var v in row.Values)
            {
                values[j++] = v;
            }
            _model.AppendValues(values);
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }

        public void Clear()
        {
            _model.Clear();
        }
    }
}

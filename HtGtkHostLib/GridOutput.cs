using com.antlersoft.HostedTools.Framework.Interface.UI;
using System.Collections.Generic;
using Gtk;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class GridOutput : ScrolledWindow, IGridOutput
    {
        ListStore _model = new ListStore(typeof(Dictionary<string,object>));
        Dictionary<string,TreeViewColumn> columns = new Dictionary<string, TreeViewColumn>();
        CellRenderer _renderer = new CellRendererText();
        TreeView _tree = new TreeView();

        internal GridOutput()
        {
            _tree.Model = _model;
            this.Add(_tree);
            SetSizeRequest(600, 200);
        }

        public void AddRow(Dictionary<string, object> row)
        {
            Gtk.Application.Invoke(delegate
            {
                foreach (var col in row.Keys)
                {
                    TreeViewColumn v;
                    if (! columns.TryGetValue(col, out v))
                    {
                        v = new TreeViewColumn(col, _renderer);
                        v.SetCellDataFunc(_renderer, DataFunc);
                        _tree.AppendColumn(v);
                        columns[col] = v;
                     }
                }
                _model.AppendValues(row);
            });
        }

        private static void DataFunc(TreeViewColumn column, CellRenderer renderer, ITreeModel model, TreeIter iter)
        {
            if (renderer is CellRendererText text)
            {
                var result = string.Empty;
                var row = model.GetValue(iter, 0) as Dictionary<string,object>;
                if (row != null)
                {
                    object obj;
                    if (row.TryGetValue(column.Title, out obj))
                    {
                        result = obj.ToString();
                    }
                }
                text.Text = result;
            }

        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }

        public void Clear()
        {
            Gtk.Application.Invoke(delegate {
                 _model.Clear();
                 foreach (var col in columns.Values)
                 {
                     _tree.RemoveColumn(col);
                 }
                 columns.Clear();
                });
        }
    }
}

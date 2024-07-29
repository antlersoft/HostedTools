using com.antlersoft.HostedTools.Framework.Interface.UI;
using System.Collections.Generic;
using Gtk;
using Gdk;
using System;
using System.Xml.XPath;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class TickHolder
    {
        public long Ticks;
    }

    class TreeViewWithButton : TreeView
    {
        internal delegate void TreeViewButtonHandler(EventButton evnt);
        internal event TreeViewButtonHandler ButtonPressedOnTree;

        protected override bool OnButtonPressEvent(EventButton evnt)
        {
            var result = base.OnButtonPressEvent(evnt);
            ButtonPressedOnTree.Invoke(evnt);
            return result;
        }
    }
    class GridOutput : ScrolledWindow, IGridOutput
    {
        ListStore _model = new ListStore(typeof(Dictionary<string,object>));
        Dictionary<string,TreeViewColumn> columns = new Dictionary<string, TreeViewColumn>();
        CellRenderer _renderer = new CellRendererText();
        TreeViewWithButton _tree = new TreeViewWithButton();
        Menu _menu;
        internal GridOutput()
        {
            _tree.Model = _model;
            this.Add(_tree);
            SetSizeRequest(600, 200);
            _menu = new Menu();
            var copy = new Gtk.MenuItem("Copy");
            copy.Activated += (sender, e) =>
            {
                System.Console.WriteLine("Copy selected");
                Gtk.Clipboard clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));

                var selection = _tree.Selection;
                selection.SelectedForeach((model, path, iter) => {
                    var row = model.GetValue(iter, 0) as Dictionary<string, object>;
                    if (row != null)
                    {
                        foreach (var v in row.Values)
                        {
                            clipboard.Text = v.ToString();
                        }
                    }
                });
            };
            _menu.Name = "Context menu";
            _menu.Add(copy);
            _tree.ButtonPressedOnTree += (btn) =>
            {
                if (btn.Button == 3)
                {
                    _menu.ShowAll();
                    _menu.Popup(null, null, null, btn.Button, btn.Time);
                }
            };
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
                        if (obj is String && ((String)obj).IndexOf("Ticks")>0) {
                            DateTime dt = new DateTime(JsonConvert.DeserializeObject<TickHolder>((String)obj).Ticks);
                            result = dt.ToString();
                        }
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

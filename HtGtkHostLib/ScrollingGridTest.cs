using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.UI;
using com.antlersoft.HostedTools.Framework.Gtk.Interface;

using Gtk;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    [Export(typeof(IPlugin))]
    [Export(typeof(IMenuItemSource))]
    public class ScrollingGridTest : HostedObjectBase, IElementSource, IPlugin, IMenuItemSource
    {
        
        ListStore _model = new ListStore(typeof(Dictionary<string,object>));
        Dictionary<string,TreeViewColumn> columns = new Dictionary<string, TreeViewColumn>();
        CellRenderer _renderer = new CellRendererText();
        TreeView _tree = new TreeView();

        ScrolledWindow _sw = new ScrolledWindow();

        GridOutput _go = new GridOutput();

        OutputPane op = new OutputPane(new OutputPaneList(EPaneListOrientation.Vertical, new IOutputPaneSpecifier[] {new OutputPaneSpecifier(EOutputPaneType.Text, null, 25), new OutputPaneSpecifier(EOutputPaneType.Grid, null, 75)}));

        public ScrollingGridTest()
        {
            //_tree.Model = _model;
            //_sw.Add(_tree);
            //AddRows(50);
        }

        void AddRows(int rows)
        {
            for (int i = 0; i<rows; i++)
            {
                var d = new Dictionary<string,object>();
                for (int j = 0; j<15; j++)
                {
                    string colHdr = $"col{j}";
                    d[colHdr] = $"row{i}-{colHdr}";
                }
                op.FindGridOutput().AddRow(d);
            }
        }

        public IEnumerable<com.antlersoft.HostedTools.Framework.Interface.Menu.IMenuItem> Items => new [] {
            new com.antlersoft.HostedTools.Framework.Model.Menu.MenuItem("Common.GridTest", "Grid Test", typeof(ScrollingGridTest).FullName, "Common")
        };
/*
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
        */
        public Widget GetElement(object container)
        {
            Task.Run(() => {

                Task.Delay(5000).Wait();
                LambdaDispatch.Run(() => {
                    AddRows(50);
                });
            });
            return op.Element;
        }

        public string Name => GetType().FullName;
    }
}
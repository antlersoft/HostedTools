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
    public class ScrollingGridTest : HostedObjectBase, IWork, IPlugin, IMenuItemSource, IOutputPaneList
    {
        
        ListStore _model = new ListStore(typeof(Dictionary<string,object>));
        Dictionary<string,TreeViewColumn> columns = new Dictionary<string, TreeViewColumn>();
        CellRenderer _renderer = new CellRendererText();
        TreeView _tree = new TreeView();

        ScrolledWindow _sw = new ScrolledWindow();

        GridOutput _go = new GridOutput();

        IOutputPaneList pl = new OutputPaneList(EPaneListOrientation.Vertical, new IOutputPaneSpecifier[] {new OutputPaneSpecifier(EOutputPaneType.Text, null, 25), new OutputPaneSpecifier(EOutputPaneType.Grid, null, 75)});

        public ScrollingGridTest()
        {
            //_tree.Model = _model;
            //_sw.Add(_tree);
            //AddRows(50);
        }

        public EPaneListOrientation Orientation => pl.Orientation;
        public IList<IOutputPaneSpecifier> Panes => pl.Panes;


        public void Perform(IWorkMonitor monitor)
        {
            int rows = 50;
            for (int i = 0; i<rows; i++)
            {
                var d = new Dictionary<string,object>();
                for (int j = 0; j<15; j++)
                {
                    string colHdr = $"col{j}";
                    d[colHdr] = $"row{i}-{colHdr}";
                }
                monitor.Cast<IHasOutputPanes>().FindGridOutput().AddRow(d);
            }
        }

        public IEnumerable<com.antlersoft.HostedTools.Framework.Interface.Menu.IMenuItem> Items => new [] {
            new com.antlersoft.HostedTools.Framework.Model.Menu.MenuItem("Common.GridTest", "Grid Test", typeof(ScrollingGridTest).FullName, "Common")
        };

        public string Name => GetType().FullName;
    }
}
using com.antlersoft.HostedTools.Framework.Gtk.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using Gtk;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    [Export(typeof(IMenuItemSource))]
    [Export(typeof(IBackgroundWorkReceiver))]
    [Export(typeof(IPlugin))]
    public class BackgroundDisplay : HostedObjectBase, IPlugin, IBackgroundWorkReceiver, IElementSource, IMenuItemSource
    {
        Notebook _tabs = new Notebook();

        public string Name
        {
            get { return GetType().FullName; }
        }

        public void AcceptWork(IWorkMonitor monitor, Widget workOutputElement, string title)
        {
            var item = new BackgroundItem(monitor, workOutputElement);
            _tabs.AppendPage(item, new Label() { Text = title });
            int pages = _tabs.NPages;
            item.OnCloseListener.AddListener(b => _tabs.RemovePage(pages));
        }

        public Widget GetElement(object container)
        {
            return _tabs;
        }

        public IEnumerable<IMenuItem> Items
        {
            get { return new[] { new Framework.Model.Menu.MenuItem("Common.BackgroundTabs", "Background Tabs", GetType().FullName, "Common") }; }
        }
    }
}

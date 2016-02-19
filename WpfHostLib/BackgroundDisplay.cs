using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Wpf.Interface;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    [Export(typeof(IMenuItemSource))]
    [Export(typeof(IBackgroundWorkReceiver))]
    [Export(typeof(IPlugin))]
    public class BackgroundDisplay : HostedObjectBase, IPlugin, IBackgroundWorkReceiver, IElementSource, IMenuItemSource
    {
        TabControl _tabs = new TabControl();

        public string Name
        {
            get { return GetType().FullName; }
        }

        public void AcceptWork(IWorkMonitor monitor, FrameworkElement workOutputElement, string title)
        {
            var item = new BackgroundItem(monitor, workOutputElement);
            TabItem tab = new TabItem { Header = title, Content = item};
            item.OnCloseListener.AddListener(b => _tabs.Items.Remove(tab));
            _tabs.Items.Add(tab);
        }

        public FrameworkElement GetElement(object container)
        {
            return _tabs;
        }

        public IEnumerable<IMenuItem> Items
        {
            get { return new [] { new Framework.Model.Menu.MenuItem("Common.BackgroundTabs", "Background Tabs", GetType().FullName, "Common") }; }
        }
    }
}

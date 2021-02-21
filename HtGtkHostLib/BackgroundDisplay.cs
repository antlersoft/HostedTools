using com.antlersoft.HostedTools.Framework.Gtk.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using Gtk;
using System;
using System.ComponentModel.Composition;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    [Export(typeof(IMenuItemSource))]
    [Export(typeof(IBackgroundWorkReceiver))]
    [Export(typeof(IPlugin))]
    public class BackgroundDisplay : HostedObjectBase, IPlugin, IBackgroundWorkReceiver, IElementSource, IMenuItemSource
    {
        public string Name => typeof(BackgroundDisplay).FullName;

        public void AcceptWork(IWorkMonitor monitor, Widget workOutputElement, string title)
        {
            throw new NotImplementedException();
        }

        public Widget GetElement(object container)
        {
            throw new NotImplementedException();
        }
    }
}

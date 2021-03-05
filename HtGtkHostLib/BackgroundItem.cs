using System;
using System.Collections.Generic;
using System.Text;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using Gtk;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class BackgroundItem : VBox
    {
        internal BackgroundItem(IWorkMonitor monitor, Widget output)
        : base(false, 2)
        {
            HBox buttons = new HBox(true, 2);
            Button cancelButton = new Button() { Label = "Cancel" };
            buttons.PackStart(cancelButton, false, false, 0);
            Button closeButton = new Button() { Label = "Close" };
            closeButton.Clicked += (sender, args) => ((ListenerCollection<bool>)OnCloseListener).NotifyListeners(true);
            OnCloseListener = new ListenerCollection<bool>();
            WorkMonitor wm = monitor.Cast<WorkMonitor>();
            if (wm != null)
            {
                wm.IsRunningChanged.AddListener(b => cancelButton.Sensitive = b);
                cancelButton.Sensitive = wm.IsRunning;
                cancelButton.Clicked += (sender, args) => { wm.IsCanceled = true; };
            }
        }

        public IListenerCollection<bool> OnCloseListener { get; private set; }
    }
}

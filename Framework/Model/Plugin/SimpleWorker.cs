using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;

namespace com.antlersoft.HostedTools.Framework.Model.Plugin
{
    public abstract class SimpleWorker : EditOnlyPlugin, IWork
    {
        protected SimpleWorker(IEnumerable<IMenuItem> menuEntries, IEnumerable<string> keys)
            : base(menuEntries, keys)
        {
        }

        protected SimpleWorker(IMenuItem item, IEnumerable<string> keys)
            : base(item, keys)
        {
            
        }

        public static void ClearMonitor(IWorkMonitor monitor)
        {
            var clearable = monitor.Cast<IClearableMonitor>();
            if (clearable != null)
            {
                clearable.Clear();
            }
        }

        public void CanBackground(IWorkMonitor monitor, string backgroundTitle)
        {
            var background = monitor.Cast<IBackgroundableMonitor>();
            if (background != null)
            {
                background.CanBackground(backgroundTitle);
            }
        }

        public abstract void Perform(IWorkMonitor monitor);
    }
}

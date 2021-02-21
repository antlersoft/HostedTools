using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using Gtk;

namespace com.antlersoft.HostedTools.Framework.Gtk.Interface
{
    public interface IBackgroundWorkReceiver : IHostedObject
    {
        void AcceptWork(IWorkMonitor monitor, Widget workOutputElement, string title);
    }
}

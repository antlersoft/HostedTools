using System.Windows;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;

namespace com.antlersoft.HostedTools.Framework.Wpf.Interface
{
    public interface IBackgroundWorkReceiver : IHostedObject
    {
        void AcceptWork(IWorkMonitor monitor, FrameworkElement workOutputElement, string title);
    }
}

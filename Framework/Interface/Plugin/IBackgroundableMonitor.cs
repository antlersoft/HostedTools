
namespace com.antlersoft.HostedTools.Framework.Interface.Plugin
{
    public interface IBackgroundableMonitor : IWorkMonitor
    {
        void CanBackground(string backgroundTitle);
    }
}

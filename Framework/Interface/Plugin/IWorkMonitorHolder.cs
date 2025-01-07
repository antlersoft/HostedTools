namespace com.antlersoft.HostedTools.Framework.Interface.Plugin
{
    public interface IWorkMonitorHolder : IHostedObject
    {
       IWorkMonitor SetMonitor(IWorkMonitor newMonitor);
       IWorkMonitor ClearMonitor();
    }
}
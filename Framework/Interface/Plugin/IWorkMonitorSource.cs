
namespace com.antlersoft.HostedTools.Framework.Interface.Plugin
{
    public interface IWorkMonitorSource : IHostedObject
    {
        /// <summary>
        /// Produce a (potentially short-lived) IWorkMonitor instance.
        /// 
        /// Not guaranteed the return the same monitor each time.  Callers should not expect to hold the monitor
        /// for a long time
        /// <para>
        /// The contract of this interface demands that it always return a valid IWorkMonitor.  It may
        /// return a do-nothing monitor, which should be identical to WorkMonitorSource.EmptyMonitor
        /// </para>
        /// </summary>
        /// <returns>Always a valid IWorkMonitorInstance, although it might not do anything</returns>
        IWorkMonitor GetMonitor();
    }
}

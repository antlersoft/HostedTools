using System;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Cloud
{
    public interface ICloudQueueing : IDisposable
    {
        ICloudQueue GetQueue(string publicName, bool fromNotification = false);
        Task<ICloudQueue> GetQueueAsync(string publicName, bool fromNotification = false);
        ICloudQueue GetQueueFromInternalName(string internalName, bool fromNotification = false);
    }
}

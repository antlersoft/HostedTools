using System;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Cloud
{
    public interface ICloudTopic : IDisposable
    {
        void SendNotification(string message, string? subject = null);
        Task SendNotificationAsync(string message, string? subject = null);
    }

    public interface ICloudNotifier : IDisposable
    {
        ICloudTopic GetTopic(string publicName, string? endpointArn = null);
        ICloudTopic GetTopicFromInternalName(string internalName, string? endpointArn = null);
    }
}

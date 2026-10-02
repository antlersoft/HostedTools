using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Cloud
{
    public interface ICloudMessage
    {
        string MessageBody { get; }
        string MessageId { get; }
        string ReceiptHandle { get; }
    }

    public interface ICloudQueue : IDisposable
    {
        string PublicName { get; }
        IHtValue GetQueueAttributes();
        Task<IHtValue> GetQueueAttributesAsync();
        IList<string> SendMessages(int sendDelaySeconds, IList<string> toSend);
        IList<string> SendMessages(int sendDelaySeconds, IList<string> toSend, CancellationToken cancellation);
        Task<IList<string>> SendMessagesAsync(int sendDelaySeconds, IList<string> toSend);
        Task<IList<string>> SendMessagesAsync(int sendDelaySeconds, IList<string> toSend, CancellationToken cancellation);
        void DeleteMessages(IList<string> receiptHandles);
        void DeleteMessages(IList<string> receiptHandles, CancellationToken cancellation);
        Task DeleteMessagesAsync(IList<string> receiptHandles);
        Task DeleteMessagesAsync(IList<string> receiptHandles, CancellationToken cancellation);

        IList<ICloudMessage> ReceiveMessages(int maximumMessages, int visibilityTimeoutSeconds = 0, int waitTimeSeconds = 0);
        Task<IList<ICloudMessage>> ReceiveMessagesAsync(int maximumMessages, int visibilityTimeoutSeconds = 0, int waitTimeSeconds = 0);
    }

    public static class CloudQueueExtensions
    {
        public static IEnumerable<ICloudMessage> GetMessageListener(this ICloudQueue queue, int visibilityTimeoutSeconds = 0,
            int waitTimeSeconds = 0)
        {
            do
            {
                IList<ICloudMessage> messages = queue.ReceiveMessages(10, visibilityTimeoutSeconds, waitTimeSeconds);
                foreach (var msg in messages)
                {
                    yield return msg;
                }
            } while (true);
        }
    }
}

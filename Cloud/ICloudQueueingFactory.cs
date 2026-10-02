using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Cloud
{
    public interface ICloudQueueingFactory
    {
        ICloudQueueing GetQueueing(IHtValue credentials);
    }
}

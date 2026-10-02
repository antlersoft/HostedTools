
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Cloud
{
    public interface ICloudNotifierFactory
    {
        ICloudNotifier GetNotifier(IHtValue credentials);
    }
}

using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Cloud
{
    public interface ICloudStorageFactory
    {
        ICloudStorage GetStorage(IHtValue credentials);
    }
}

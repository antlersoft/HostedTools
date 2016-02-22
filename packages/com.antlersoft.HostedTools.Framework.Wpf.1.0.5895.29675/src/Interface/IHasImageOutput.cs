using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Framework.Wpf.Interface
{
    public interface IHasImageOutput : IHostedObject
    {
        IImageOutput FindImageOutput(string title = null);
    }
}

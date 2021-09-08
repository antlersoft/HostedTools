using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Archive.Interface
{
    public interface IArchiveFilterFactory : IHostedObject
    {
        string Name { get; }
        IArchiveFilter ConfigureFilter(IHtValue configuration);
    }
}

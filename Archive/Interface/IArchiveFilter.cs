using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Archive.Interface
{
    public interface IArchiveFilter : IHostedObject
    {
        IArchive GetFilteredArchive(IArchive underlying);
    }
}

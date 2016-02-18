using System;
namespace com.antlersoft.HostedTools.Interface
{
    public interface IOdsLogProviderFactory
    {
        IHtLogProvider GetLogProvider(string name);
    }
}


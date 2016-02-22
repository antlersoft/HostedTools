using System;
namespace com.antlersoft.HostedTools.Interface
{
    public interface IHtLogProviderFactory
    {
        IHtLogProvider GetLogProvider(string name);
    }
}


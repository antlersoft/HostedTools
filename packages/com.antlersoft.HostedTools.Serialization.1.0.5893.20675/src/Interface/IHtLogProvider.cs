using System;
namespace com.antlersoft.HostedTools.Interface
{
	/// <summary>
	/// An IHtLog can be implemented simply in terms of an IHtLogProvider
	/// </summary>
    public interface IHtLogProvider
    {
        bool IsLogging(ELogLevel level);
        void Log(string category, string requestId, ELogLevel level, string message, Exception ex, object additional);
    }
}


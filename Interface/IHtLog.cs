using System;

namespace com.antlersoft.HostedTools.Interface
{
    public enum ELogLevel
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error,
        Severe,
        Off
    }

    public delegate string HtLogFormatter(string format, params object[] parameters);

    public delegate string FormatCallback(HtLogFormatter formatter);

	/// <summary>
	/// Provides a common interface to different, back-end log providers.
	/// <para>A given instance of IHtLog is for a specifc category, and optionally a specific request id.  The category is supposed to select
	/// logging format and destination; the request id represents common data for all messages on that instance.</para>
	/// </summary>
    public interface IHtLog
    {
        string Category { get; }
        string RequestId { get; set; }
        bool IsTrace { get; }
        bool IsDebug { get; }
        bool IsInfo { get; }
        bool IsWarning { get; }
        bool IsError { get; }
        bool IsSevere { get; }
        void Log(string category, string requestId, ELogLevel level, string message, Exception ex, object additional);
        void Trace(string msg, Exception ex = null, object additional = null);
        void Trace(FormatCallback callback, Exception ex = null, object additional = null);
        void Trace(string requestId, string msg, Exception ex = null, object additional = null);
        void Trace(string requestId, FormatCallback callback, Exception ex = null, object additional = null);
        void Debug(string msg, Exception ex = null, object additional = null);
        void Debug(FormatCallback callback, Exception ex = null, object additional = null);
        void Debug(string requestId, string msg, Exception ex = null, object additional = null);
        void Debug(string requestId, FormatCallback callback, Exception ex = null, object additional = null);
        void Info(string msg, Exception ex = null, object additional = null);
        void Info(FormatCallback callback, Exception ex = null, object additional = null);
        void Info(string requestId, string msg, Exception ex = null, object additional = null);
        void Info(string requestId, FormatCallback callback, Exception ex = null, object additional = null);
        void Warning(string msg, Exception ex = null, object additional = null);
        void Warning(FormatCallback callback, Exception ex = null, object additional = null);
        void Warning(string requestId, string msg, Exception ex = null, object additional = null);
        void Warning(string requestId, FormatCallback callback, Exception ex = null, object additional = null);
        void Error(string msg, Exception ex = null, object additional = null);
        void Error(FormatCallback callback, Exception ex = null, object additional = null);
        void Error(string requestId, string msg, Exception ex = null, object additional = null);
        void Error(string requestId, FormatCallback callback, Exception ex = null, object additional = null);
        void Severe(string msg, Exception ex = null, object additional = null);
        void Severe(FormatCallback callback, Exception ex = null, object additional = null);
        void Severe(string requestId, string msg, Exception ex = null, object additional = null);
        void Severe(string requestId, FormatCallback callback, Exception ex = null, object additional = null);
    }

	/// <summary>
	/// An implementation of IHtLogManager provides IHtLog instances
	/// </summary>
    public interface IHtLogManager
    {
        IHtLog GetLog(string category = null, string requestId = null);
    }
}


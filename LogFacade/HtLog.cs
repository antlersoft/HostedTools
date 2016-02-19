using System;

using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.LogFacade
{
    public class HtLog : IHtLog
    {
        private string _category;
        private IHtLogProvider _provider;

        public HtLog(IHtLogProvider provider, string category, string requestId)
        {
            _provider = provider;
            _category = category;
            RequestId = requestId;
        }

        public string Category
        {
            get { return _category; }
        }

        public string RequestId { get; set; }

        public bool IsTrace
        {
            get { return _provider.IsLogging(ELogLevel.Trace); }
        }

        public bool IsDebug
        {
            get { return _provider.IsLogging(ELogLevel.Debug); }
        }

        public bool IsInfo
        {
            get { return _provider.IsLogging(ELogLevel.Info); }
        }

        public bool IsWarning
        {
            get { return _provider.IsLogging(ELogLevel.Warning); }
        }

        public bool IsError
        {
            get { return _provider.IsLogging(ELogLevel.Error); }
        }

        public bool IsSevere
        {
            get { return _provider.IsLogging(ELogLevel.Severe); }
        }

        public void Log(string category, string requestId, ELogLevel level, string message, Exception ex, object additional)
        {
            _provider.Log(category, requestId, level, message, ex, additional);
        }

        private void Log(string category, string requestId, ELogLevel level, FormatCallback callback, Exception ex,
            object additional)
        {
            if (_provider.IsLogging(level))
            {
                Log(category, requestId, level, callback(String.Format), ex, additional);
            }
        }

        public void Trace(string msg, Exception ex = null, object additional = null)
        {
            Log(_category, RequestId, ELogLevel.Trace, msg, ex, additional);
        }

        public void Trace(FormatCallback callback, Exception ex = null, object additional = null)
        {
            Log(_category, RequestId, ELogLevel.Trace, callback, ex, additional);
        }

        public void Trace(string requestId, string msg, Exception ex = null, object additional = null)
        {
            Log(_category, requestId, ELogLevel.Trace, msg, ex, additional);
        }

        public void Trace(string requestId, FormatCallback callback, Exception ex = null, object additional = null)
        {
            Log(_category, requestId, ELogLevel.Trace, callback, ex, additional);
        }

        public void Debug(string msg, Exception ex = null, object additional = null)
        {
            Log(_category, RequestId, ELogLevel.Debug, msg, ex, additional);
        }

        public void Debug(FormatCallback callback, Exception ex = null, object additional = null)
        {
            Log(_category, RequestId, ELogLevel.Debug, callback, ex, additional);
        }

        public void Debug(string requestId, string msg, Exception ex = null, object additional = null)
        {
            Log(_category, requestId, ELogLevel.Debug, msg, ex, additional);
        }

        public void Debug(string requestId, FormatCallback callback, Exception ex = null, object additional = null)
        {
            Log(_category, requestId, ELogLevel.Debug, callback, ex, additional);
        }

        public void Info(string msg, Exception ex = null, object additional = null)
        {
            Log(_category, RequestId, ELogLevel.Info, msg, ex, additional);
        }

        public void Info(FormatCallback callback, Exception ex = null, object additional = null)
        {
            Log(_category, RequestId, ELogLevel.Info, callback, ex, additional);
        }

        public void Info(string requestId, string msg, Exception ex = null, object additional = null)
        {
            Log(_category, requestId, ELogLevel.Info, msg, ex, additional);
        }

        public void Info(string requestId, FormatCallback callback, Exception ex = null, object additional = null)
        {
            Log(_category, requestId, ELogLevel.Info, callback, ex, additional);
        }

        public void Warning(string msg, Exception ex = null, object additional = null)
        {
            Log(_category, RequestId, ELogLevel.Warning, msg, ex, additional);
        }

        public void Warning(FormatCallback callback, Exception ex = null, object additional = null)
        {
            Log(_category, RequestId, ELogLevel.Warning, callback, ex, additional);
        }

        public void Warning(string requestId, string msg, Exception ex = null, object additional = null)
        {
            Log(_category, requestId, ELogLevel.Warning, msg, ex, additional);
        }

        public void Warning(string requestId, FormatCallback callback, Exception ex = null, object additional = null)
        {
            Log(_category, requestId, ELogLevel.Warning, callback, ex, additional);
        }

        public void Error(string msg, Exception ex = null, object additional = null)
        {
            Log(_category, RequestId, ELogLevel.Error, msg, ex, additional);
        }

        public void Error(FormatCallback callback, Exception ex = null, object additional = null)
        {
            Log(_category, RequestId, ELogLevel.Error, callback, ex, additional);
        }

        public void Error(string requestId, string msg, Exception ex = null, object additional = null)
        {
            Log(_category, requestId, ELogLevel.Error, msg, ex, additional);
        }

        public void Error(string requestId, FormatCallback callback, Exception ex = null, object additional = null)
        {
            Log(_category, requestId, ELogLevel.Error, callback, ex, additional);
        }

        public void Severe(string msg, Exception ex = null, object additional = null)
        {
            Log(_category, RequestId, ELogLevel.Severe, msg, ex, additional);
        }

        public void Severe(FormatCallback callback, Exception ex = null, object additional = null)
        {
            Log(_category, RequestId, ELogLevel.Severe, callback, ex, additional);
        }

        public void Severe(string requestId, string msg, Exception ex = null, object additional = null)
        {
            Log(_category, requestId, ELogLevel.Severe, msg, ex, additional);
        }

        public void Severe(string requestId, FormatCallback callback, Exception ex = null, object additional = null)
        {
            Log(_category, requestId, ELogLevel.Severe, callback, ex, additional);
        }
    }
}

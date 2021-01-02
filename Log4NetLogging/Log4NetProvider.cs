using System;
using System.Text;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.LogFacade;
using log4net;

namespace com.antlersoft.HostedTools.Log4NetLogging
{
    internal class Log4NetProvider : IHtLogProvider
    {
        private ILog _log = null;

        internal Log4NetProvider(string logName)
        {
			if (string.IsNullOrWhiteSpace(logName))
			{
            	logName = Logging.AppConfig.Get("com.antlersoft.HostedTools.Logging.LogProviderName", null);
			}
            if (string.IsNullOrWhiteSpace(logName))
            {
                logName = "Log4NetProvider";
            }

            log4net.Config.XmlConfigurator.Configure();
            _log = LogManager.GetLogger(logName);
        }

        public bool IsLogging(ELogLevel level)
        {
            if (_log == null)
            {
                return false;
            }

            switch (level)
            {
                case ELogLevel.Debug:
                    return _log.IsDebugEnabled;

                case ELogLevel.Error:
                    return _log.IsErrorEnabled || _log.IsFatalEnabled;

                case ELogLevel.Info:
                    return _log.IsInfoEnabled;

                case ELogLevel.Warning:
                    return _log.IsWarnEnabled;

                default:
                    return true;
            }
            
        }

        public void Log(string category, string requestId, ELogLevel level, string message, Exception ex, object additional)
        {
            if (_log == null)
                return;

            string text = message;
            if (category != null || requestId != null || (level <= ELogLevel.Info && ex != null) || additional != null)
            {
                var sb = new StringBuilder();
                Logging.DefaultMessageFormat(sb, category, requestId, level, message, ex, additional);
                text = sb.ToString();
            }

            switch (level)
            {
                case ELogLevel.Debug:
                case ELogLevel.Trace:
                    _log.Debug(text);
                    break;

                case ELogLevel.Error:
                    _log.Error(text, ex);
                    break;

                case ELogLevel.Severe:
                    _log.Fatal(text, ex);
                    break;

                case ELogLevel.Info:
                    _log.Info(text, ex);
                    break;

                case ELogLevel.Warning:
                    _log.Warn(text, ex);
                    break;

                default:
                    _log.Warn(text, ex);
                    break;
            }

        }
    }
}

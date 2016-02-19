using System;
using System.Text;
using com.antlersoft.HostedTools.Interface;
using System.Diagnostics;

namespace com.antlersoft.HostedTools.LogFacade
{
    internal class TraceProviderFactory : IHtLogProviderFactory
    {
        TraceLogProvider _provider = new TraceLogProvider();
        class TraceLogProvider : IHtLogProvider
        {
            internal TraceLogProvider()
            {
                Trace.AutoFlush = true;
            }
            public bool IsLogging(ELogLevel level)
            {
                return true;
            }

            public void Log(string category, string requestId, ELogLevel level, string message, Exception ex, object additional)
            {
                string text = message;
                if (category != null || requestId != null || ex != null || additional != null)
                {
                    StringBuilder sb = new StringBuilder();
                    Logging.DefaultMessageFormat(sb, null, requestId, level, message, ex, additional);
                    text = sb.ToString();
                }
                Trace.WriteLine(text, category);
            }
        }

        public IHtLogProvider GetLogProvider(string name)
        {
            return _provider;
        }
    }
}

using System;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Log4NetLogging
{
    public class Log4NetProviderFactory : IHtLogProviderFactory
    {
        private Log4NetProvider _provider = new Log4NetProvider(null);

        public Log4NetProviderFactory()
        {
            
        }

        public IHtLogProvider GetLogProvider(string name)
        {
            return _provider;
        }
    }
}

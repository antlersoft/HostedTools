
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.LogFacade
{
    internal class LogManager : IHtLogManager
    {
        private IHtLogProviderFactory _providerFactory;

        internal LogManager(IHtLogProviderFactory factory)
        {
            _providerFactory = factory;
        }

        public IHtLog GetLog(string category = null, string requestId = null)
        {
            return new HtLog(_providerFactory.GetLogProvider(category), category, requestId);
        }
    }
}

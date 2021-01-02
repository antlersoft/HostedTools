using System;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;

using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.LogFacade
{
    public static class Logging
    {
        private static object _lock = new object();
        private static IHtLogManager _manager;
        private static IHtLogProviderFactory _factory;
        private static IAppConfig _configuration;
        public static IHtLogManager Manager
        {
            get
            {
                var factory = ProviderFactory;
                Exception factoryCreationException = null;
                if (factory == null || _manager == null)
                {
                    lock (_lock)
                    {
                        if (factory == null && _configuration != null)
                        {
                            string providerName =
                                _configuration.Get("com.antlersoft.HostedTools.LogFacade.ProviderFactoryClass", (string)null);
                            if (providerName != null)
                            {
                                try
                                {
                                    var type = Type.GetType(providerName);
                                    if (type != null)
                                    {
                                        ConstructorInfo constructor = type.GetConstructor(new Type[0]);
                                        if (constructor != null)
                                        {
                                            factory =
                                                (IHtLogProviderFactory)
                                                    constructor.Invoke(new object[0]);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    factoryCreationException = ex;
                                }
                            }
                            if (factory == null)
                            {
                                factory = new TraceProviderFactory();
                            }
                        }
                        _factory = factory;
                        if (_manager == null)
                        {
                            _manager = new LogManager(factory);
                        }
                    }
                }
                if (factoryCreationException != null)
                {
                    _manager.GetLog("com.antlersoft.HostedTools.LogFacade.Logging").Error("Error creating provider factory",
                        factoryCreationException);
                }
                return _manager;
            }
        }

        public static IHtLogProviderFactory ProviderFactory
        {
            get
            {
                lock (_lock)
                {
                    return _factory;
                }
            }
            set {
                lock (_lock)
                {
                    if (value != _factory)
                    {
                        _factory = value;
                        _manager = null;
                    }
                }
            }
        }

        public static IAppConfig AppConfig
        {
            get
            {
                 return _configuration;
            }
            set
            {
                 _configuration = value;
            }
        }
        public static void DefaultExceptionFormat(StringBuilder sb, Exception ex)
        {
            if (ex != null)
            {
                sb.Append("\r\n");
                sb.Append(ex.Message);
                sb.Append("\r\n");
                sb.Append(ex.StackTrace);
                for (Exception inner = ex.InnerException; inner != null; inner = inner.InnerException)
                {
                    sb.Append("Caused by:\r\n");
                    sb.Append(inner.Message);
                    sb.Append("\r\n");
                    sb.Append(inner.StackTrace);
                }
            }
        }

        public static void DefaultMessageFormat(StringBuilder sb, string category, string requestId, ELogLevel level,
            string message, Exception ex, object additional)
        {
            if (category != null)
            {
                sb.Append(category);
                sb.Append(':');
            }
            if (requestId != null)
            {
                sb.Append(requestId);
                sb.Append(':');
            }
            sb.Append(message);
            DefaultExceptionFormat(sb, ex);
            if (additional != null)
            {
                sb.Append("\r\n");
                sb.Append(JsonConvert.SerializeObject(additional));
            }
        }
    }
}

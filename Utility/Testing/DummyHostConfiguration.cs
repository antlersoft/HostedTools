using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Utility.Testing
{
    public class DummyHostConfiguration : IConfiguration
    {
        class DummyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
        class DummyChangeToken : IChangeToken
        {
            public bool HasChanged => false;

            public bool ActiveChangeCallbacks => false;

            public IDisposable RegisterChangeCallback(Action<object> callback, object state)
            {
                return new DummyDisposable();
            }
        }
        public string this[string key] {
            get { return null;  }
            set { }
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return new IConfigurationSection[0];
        }

        public IChangeToken GetReloadToken()
        {
            return new DummyChangeToken();
        }

        public IConfigurationSection GetSection(string key)
        {
            return null;
        }
    }
}

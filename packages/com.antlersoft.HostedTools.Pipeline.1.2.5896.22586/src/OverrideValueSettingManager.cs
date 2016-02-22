using System;
using System.Collections.Generic;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Pipeline
{
    public class OverrideValueSettingManager : ISettingManager
    {
        private ISettingManager _underlying;
        // Lazily populated...
        private Dictionary<string,ISettingScope> _scopes;
        private Dictionary<string, string> _overrides;
 
        public OverrideValueSettingManager(ISettingManager underlying, Dictionary<string,string> overrides)
        {
            _underlying = underlying;
            _overrides = overrides;
            _scopes = new Dictionary<string, ISettingScope>();
        }

        public ISetting this[string key]
        {
            get
            {
                int index = key.LastIndexOf('.');
                if (index >= 0)
                {
                    string scopeKey = key.Substring(0, index);
                    ISettingScope scope = Scope(scopeKey);
                    return scope[key.Substring(index + 1)];
                }
                throw new ArgumentException("key");
            }
        }

        public ISettingScope Scope(string key)
        {
            ISettingScope result;
            lock (_scopes)
            {
                if (! _scopes.TryGetValue(key, out result))
                {
                    result = new OverrideValueSettingScope(this, _underlying.Scope(key));
                    _scopes[key] = result;
                }
            }
            return result;
        }

        public IEnumerable<ISettingScope> Scopes
        {
            get { return _underlying.Scopes.Select(s => Scope(s.ScopeKey)); }
        }

        public ISetting CreateSetting(ISettingDefinition definition)
        {
            throw new NotImplementedException();
        }

        public string GetExpansion(string unexpanded, ISettingScope scope = null)
        {
            return _underlying.GetExpansion(unexpanded, scope);
        }

        public void Save()
        {
            _underlying.Save();
        }

        internal bool TryGetOverrideRaw(string key, out string val)
        {
            return _overrides.TryGetValue(key, out val);
        }
    }
}

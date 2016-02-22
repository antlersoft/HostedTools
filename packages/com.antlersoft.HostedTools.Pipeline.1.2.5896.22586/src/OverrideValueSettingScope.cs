using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Pipeline
{
    internal class OverrideValueSettingScope : ISettingScope
    {
        private Dictionary<string,OverrideValueSetting> _settings;
        private ISettingScope _underlyingScope;
        private OverrideValueSettingManager _manager;

        internal OverrideValueSettingScope(OverrideValueSettingManager manager, ISettingScope underlyingScope)
        {
            _manager = manager;
            _settings = new Dictionary<string, OverrideValueSetting>();
            _underlyingScope = underlyingScope;
        }

        public string ScopeKey
        {
            get { return _underlyingScope.ScopeKey; }
        }

        public ISetting this[string name]
        {
            get
            {
                OverrideValueSetting s;
                if (!_settings.TryGetValue(name, out s))
                {
                    s = new OverrideValueSetting(_manager, this, _underlyingScope[name], name);
                }
                return s;
            }
        }

        public IEnumerable<ISetting> Settings
        {
            get { return _settings.Values; }
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }
    }
}

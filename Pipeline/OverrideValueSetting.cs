using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Pipeline
{
    internal class OverrideValueSetting : ISetting
    {
        private ISettingScope _scope;
        private ISetting _underlying;
        private OverrideValueSettingManager _manager;
        private string _overrideKey;

        internal OverrideValueSetting(OverrideValueSettingManager manager, ISettingScope scope, ISetting underlying, string key)
        {
            _manager = manager;
            _scope = scope;
            _underlying = underlying;
            _overrideKey = scope.ScopeKey + "." + key;
        }

        public ISettingDefinition Definition
        {
            get { return _underlying.Definition; }
        }

        public ISettingScope Scope
        {
            get { return _scope; }
        }

        public T Get<T>()
        {
            string val = GetExpanded();
            if (typeof(Enum).IsAssignableFrom(typeof(T)))
            {
                return (T)Enum.Parse(typeof(T), val);
            }
            return (T)Convert.ChangeType(val, typeof(T));
        }

        public void SetRaw(string rawValue)
        {
            _underlying.SetRaw(rawValue);
        }

        public string GetRaw()
        {
            string raw;
            if (! _manager.TryGetOverrideRaw(_overrideKey, out raw))
            {
                raw = _underlying.GetRaw();
            }
            return raw;
        }

        public string GetExpanded()
        {
            string result = GetRaw();
            if (Definition.UseExpansion)
            {
                result = _manager.GetExpansion(result, Scope);
            }
            return result;
        }

        public List<string> PreviousValues
        {
            get { return _underlying.PreviousValues; }
        }

        public IListenerCollection<ISetting> SettingChangedListeners
        {
            get { return _underlying.SettingChangedListeners; }
        }
    }
}

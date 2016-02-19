using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Model.Setting.Internal
{
    class SettingScope : HostedObjectBase, IInternalScope
    {
        private readonly string _name;
        private readonly Dictionary<string, Setting> _settings;
 
        internal SettingScope(string name)
        {
            _name = name;
            _settings = new Dictionary<string, Setting>();
        }

        #region ISettingScope implementation
        public string ScopeKey
        {
            get { return _name; }
        }

        public virtual ISetting this[string name]
        {
            get { return _settings[name]; }
        }

        public IEnumerable<ISetting> Settings
        {
            get { return _settings.Values; }
        }
        #endregion

        #region IInternalSetting implementation
        public virtual bool TryGetSetting(string key, out Setting setting)
        {
            return _settings.TryGetValue(key, out setting);
        }

        public void AddSetting(string key, Setting setting)
        {
            _settings[key] = setting;
        }
        #endregion
    }
}

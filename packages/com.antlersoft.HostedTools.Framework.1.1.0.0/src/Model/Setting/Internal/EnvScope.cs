using System;
using System.Collections.Generic;
using System.Globalization;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Model.Setting.Internal
{
    class EnvScope : SettingScope
    {
        internal const string Key = "Env";
        internal const string CurrentUnixTimeKey = "CurrentUnixTime";
        internal static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private ISettingManager _manager;

        class CurrentUnixTime : Setting
        {
            internal CurrentUnixTime(ISettingManager manager, ISettingScope scope)
                : base(manager, scope, new SecureSettingDefinition(CurrentUnixTimeKey, Key, "Current Unix Time", null, typeof(long), null, false, 0, false))
            {
            }

            public override string GetRaw()
            {
                return ((long)(DateTime.UtcNow - UnixEpoch).TotalSeconds).ToString(CultureInfo.InvariantCulture);
            }
        }

        internal EnvScope(ISettingManager manager)
            : base(Key)
        {
            _manager = manager;
            AddSetting(CurrentUnixTimeKey, new CurrentUnixTime(manager, this));
        }

        public override bool TryGetSetting(string key, out Setting setting)
        {
            if (base.TryGetSetting(key, out setting))
            {
                return true;
            }
            var value = Environment.GetEnvironmentVariable(key);
            if (value == null)
            {
                return false;
            }
            setting = new Setting(_manager, this, new SecureSettingDefinition(key, Key));
            setting.SetRaw(value);
            AddSetting(key, setting);
            return true;
        }

        public override ISetting this[string name]
        {
            get {
                Setting result;
                if (! TryGetSetting(name, out result))
                {
                    throw new KeyNotFoundException("No environment variable found with name "+name);
                }
                return result;
            }
        }
    }
}

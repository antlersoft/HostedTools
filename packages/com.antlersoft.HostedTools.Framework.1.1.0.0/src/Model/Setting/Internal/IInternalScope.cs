using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Model.Setting.Internal
{
    interface IInternalScope : ISettingScope
    {
        bool TryGetSetting(string key, out Setting setting);
        void AddSetting(string key, Setting setting);
    }
}

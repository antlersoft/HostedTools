using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Setting
{
    public interface ISettingManager
    {
        ISetting this[string key] { get; }
        ISettingScope Scope(string key);
        IEnumerable<ISettingScope> Scopes { get; }
        ISetting CreateSetting(ISettingDefinition definition);
        string GetExpansion(string unexpanded, ISettingScope scope = null);
        void Save();
    }
}

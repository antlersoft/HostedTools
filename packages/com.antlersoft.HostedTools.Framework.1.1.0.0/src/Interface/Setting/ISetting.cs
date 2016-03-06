using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Setting
{
    public interface ISetting
    {
        ISettingDefinition Definition
        {
            get;
        }

        ISettingScope Scope { get; }

        T Get<T>();
        void SetRaw(string rawValue);

        string GetRaw();

        string GetExpanded();

        List<string> PreviousValues { get; }

        IListenerCollection<ISetting> SettingChangedListeners { get; }
    }
}

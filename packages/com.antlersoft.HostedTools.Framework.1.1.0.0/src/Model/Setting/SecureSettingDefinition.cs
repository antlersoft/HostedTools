
using System;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Model.Setting
{
    public class SecureSettingDefinition : SimpleSettingDefinition, ISettingSecurity
    {
        public SecureSettingDefinition(string name, string scopeKey="", string prompt=null, string description=null, Type type=null, string defaultRaw=null, bool useExpansion=false, int numberOfPrevious=0, bool isPassword=true, bool saveToFile=false)
            : base(name, scopeKey, prompt, description, type, defaultRaw, useExpansion, numberOfPrevious)
        {
            SaveToFile = saveToFile;
            IsPassword = isPassword;
        }

        public bool SaveToFile { get; private set; }

        public bool IsPassword { get; private set; }
    }
}

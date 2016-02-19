using System;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Model.Setting
{
    public class SimpleSettingDefinition : HostedObjectBase, ISettingDefinition
    {
        public SimpleSettingDefinition(string name, string scopeKey = "", string prompt = null,
                                       string description = null, Type type = null,
                                       string defaultRaw = null, bool useExpansion = true, int numberOfPrevious = 10)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            Name = name;
            Type = type ?? typeof (string);
            ScopeKey = scopeKey;
            UseExpansion = useExpansion;
            Prompt = prompt ?? name;
            Description = description ?? Prompt;
            DefaultRaw = defaultRaw ?? string.Empty;
            NumberOfPreviousValues = numberOfPrevious;
        }

        public Type Type { get; private set; }

        public bool UseExpansion { get; private set; }

        public string ScopeKey { get; private set; }

        public string Name { get; private set; }

        public string Prompt { get; private set; }

        public string Description { get; private set; }

        public string DefaultRaw { get; private set; }

        public int NumberOfPreviousValues { get; private set; }
    }
}

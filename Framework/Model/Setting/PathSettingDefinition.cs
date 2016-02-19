using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Model.Setting
{
    public class PathSettingDefinition : SimpleSettingDefinition, IPathSettingDefinition
    {
        public PathSettingDefinition(string name, string scopeKey="", string prompt=null, bool isSave=false, bool isFolder=false, string extensions=null, string description=null, string defaultRaw=null, bool useExpansion=true, int numberOfPrevious=10)
            : base(name, scopeKey, prompt, description, typeof(string), defaultRaw, useExpansion, numberOfPrevious)
        {
            IsFolder = isFolder;
            IsSave = isSave;
            FileTypesAndExtensions = extensions;
        }

        public bool IsFolder { get; private set; }

        public bool IsSave { get; private set; }

        public string FileTypesAndExtensions { get; private set; }
    }
}

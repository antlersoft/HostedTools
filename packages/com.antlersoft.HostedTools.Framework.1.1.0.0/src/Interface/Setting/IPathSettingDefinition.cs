using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Framework.Interface.Setting
{
    public interface IPathSettingDefinition : ISettingDefinition
    {
        bool IsFolder { get; }
        bool IsSave { get; }
        /// <summary>
        /// File Type|*.ext;*.ext2|File Type 2|*.ext3;*.ext4
        /// </summary>
        string FileTypesAndExtensions { get; }
    }
}

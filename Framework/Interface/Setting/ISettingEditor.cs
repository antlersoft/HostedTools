using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Interface.Setting
{
    public interface ISettingEditor : IHostedObject
    {
        ISetting Setting { get; }
        string RawValue { get; }
    }
}
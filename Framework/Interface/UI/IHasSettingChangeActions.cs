using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Interface.UI
{
    public interface IHasSettingChangeActions : IHostedObject
    {
        Dictionary<string, Action<IWorkMonitor, ISetting>> ActionsBySettingKey { get; } 
    }
}

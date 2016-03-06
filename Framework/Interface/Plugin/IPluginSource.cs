using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Framework.Interface.Plugin
{
    /// <summary>
    /// A part that provides a number of plugins to the Plugin Manager;
    /// used when a single class provided multiple plugins
    /// </summary>
    public interface IPluginSource : IHostedObject
    {
        IEnumerable<IPlugin> SourcePlugins { get; }
    }
}

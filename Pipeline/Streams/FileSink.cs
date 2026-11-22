using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Pipeline.Extensions;

namespace com.antlersoft.HostedTools.Pipeline.Streams
{
    [Export(typeof(ISettingDefinitionSource))]
    public class FileSink : EditOnlyPlugin, ISinkStream, ISettingDefinitionSource
    {
        [Import]
        public IPluginManager PluginManager;
        
        static ISettingDefinition OutputFile = new PathSettingDefinition("OutputFile", "FileSink", "Output file", true, false, null, "Output to file");

        public FileSink()
        : base(Array.Empty<IMenuItem>(), new [] { OutputFile.FullKey() })
        {}
        public string NodeDescription => "Output to file "+SettingManager[OutputFile.FullKey()].Get<string>();

        public IEnumerable<ISettingDefinition> Definitions => new [] { OutputFile };

        public PluginState GetPluginState(IDictionary<string, PluginState> visited = null)
        {
            return this.AssemblePluginState(PluginManager, SettingManager, visited);
        }

        public Stream GetStream(PluginState state)
        {
            return new FileStream(state.SettingValues[OutputFile.FullKey()], FileMode.Create, FileAccess.Write);
        }

        public void SetPluginState(PluginState state, ISet<string> visited = null)
        {
            this.DeployPluginState(state, PluginManager, SettingManager, visited);
        }
    }
}
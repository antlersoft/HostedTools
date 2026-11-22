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
    public class FileSource : EditOnlyPlugin, IStreamSource, ISettingDefinitionSource
    {
        [Import]
        public IPluginManager PluginManager;
        
        static ISettingDefinition InputFile = new PathSettingDefinition("InputFile", "FileSource", "Input file", false, false, null, "Input from file");

        public FileSource()
        : base(Array.Empty<IMenuItem>(), new [] { InputFile.FullKey() })
        {}
        public string NodeDescription => "Input from file "+SettingManager[InputFile.FullKey()].Get<string>();

        public IEnumerable<ISettingDefinition> Definitions => new [] { InputFile };

        public PluginState GetPluginState(IDictionary<string, PluginState> visited = null)
        {
            return this.AssemblePluginState(PluginManager, SettingManager, visited);
        }

        public Stream GetStream(PluginState state)
        {
            return new FileStream(state.SettingValues[InputFile.FullKey()], FileMode.Open, FileAccess.Read);
        }

        public void SetPluginState(PluginState state, ISet<string> visited = null)
        {
            this.DeployPluginState(state, PluginManager, SettingManager, visited);
        }
    }
}
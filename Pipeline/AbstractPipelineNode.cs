using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Pipeline.Extensions;

namespace com.antlersoft.HostedTools.Pipeline {
    public abstract class AbstractPipelineNode : EditOnlyPlugin, IPipelineNode, IHasSaveKey {
        [Import]
        public IPluginManager PluginManager {get; set;}
        
        protected AbstractPipelineNode(IEnumerable<IMenuItem> menuEntries, IEnumerable<string> keys)
        : base(menuEntries, keys)
        {
        }

        protected AbstractPipelineNode(IMenuItem item, IEnumerable<string> keys)
            : base(item, keys)
        {
            
        }

        public abstract string NodeDescription { get; }

        public string SaveKey { get; set;}

        public PluginState GetPluginState(ISet<string> visited = null)
        {
            return this.AssemblePluginState(PluginManager, SettingManager, visited);
        }

        public void SetPluginState(PluginState state, ISet<string> visited = null)
        {
            this.DeployPluginState(state, PluginManager, SettingManager, visited);
        }
    }
}
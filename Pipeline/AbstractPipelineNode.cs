using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Pipeline.Extensions;

namespace com.antlersoft.HostedTools.Pipeline {
    public abstract class AbstractPipelineNode : EditOnlyPlugin, IPipelineNode {
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

        public PluginState GetPluginState()
        {
            return this.AssemblePluginState(PluginManager, SettingManager);
        }
    }
}
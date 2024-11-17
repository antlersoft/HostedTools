using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Pipeline {
    public interface IPipelineNode : IHostedObject {
        string NodeDescription { get;}
        PluginState GetPluginState(ISet<string> visited = null);
        void SetPluginState(PluginState state, ISet<string> visited = null);
    }
}
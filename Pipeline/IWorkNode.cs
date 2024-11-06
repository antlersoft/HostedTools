using com.antlersoft.HostedTools.Framework.Interface.Plugin;

namespace com.antlersoft.HostedTools.Pipeline {
    public interface IWorkNode : IPipelineNode {
        void Perform(PluginState state, IWorkMonitor monitor);
    }
}
using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Pipeline {
    public interface IPipelineNode : IHostedObject {
        string NodeDescription { get;}
        PluginState GetPluginState();
    }
}
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline {
    public class SavedNode {
        public string Name {get; set;}
        public PluginState State {get; set;}
        public IHtValue MetaData {get; set;}
    }
}
namespace com.antlersoft.HostedTools.Pipeline {
    public interface IHtValueLeaf : ILeafNode {
        IHtValueSink GetHtValueSink(PluginState state);
    }
}
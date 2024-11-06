namespace com.antlersoft.HostedTools.Pipeline {
    public interface IHtValueRoot : IRootNode {
        IHtValueSource GetHtValueSource(PluginState state);
    }
}
namespace com.antlersoft.HostedTools.Pipeline {
    public interface IHtValueStem : IStemNode {
        IHtValueTransform GetHtValueTransform(PluginState state);
    }
}
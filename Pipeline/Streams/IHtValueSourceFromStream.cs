using System.IO;

namespace com.antlersoft.HostedTools.Pipeline.Streams {
    public interface IHtValueSourceFromStream : IPipelineNode
    {
        IHtValueSource GetHtValueSource(PluginState state, Stream stream);
    }
}
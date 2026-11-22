using System.IO;

namespace com.antlersoft.HostedTools.Pipeline.Streams
{
    public interface ISinkStreamTransform : IPipelineNode
    {
        Stream GetStream(PluginState state, Stream sinkStream);
    }
}
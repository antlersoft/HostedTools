using System.IO;

namespace com.antlersoft.HostedTools.Pipeline.Streams
{
    public interface ISinkStream : IPipelineNode
    {
        Stream GetStream(PluginState state);
    }
}
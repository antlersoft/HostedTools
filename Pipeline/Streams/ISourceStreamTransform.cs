using System.IO;

namespace com.antlersoft.HostedTools.Pipeline.Streams
{
    /// <summary>
    /// Get a new stream you can read from by applying an operation to an underlying stream
    /// </summary>
    public interface ISourceStreamTransform : IPipelineNode
    {
        Stream GetStream(PluginState state, Stream inputStream);
    }
}
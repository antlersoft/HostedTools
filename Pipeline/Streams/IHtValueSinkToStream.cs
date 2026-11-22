using System.IO;

namespace com.antlersoft.HostedTools.Pipeline.Streams
{
    public interface IHtValueSinkToStream : IPipelineNode
    {
        IHtValueSink GetHtValueSink(PluginState state, Stream stream);
    }    
}
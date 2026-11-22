using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Pipeline.Extensions;

namespace com.antlersoft.HostedTools.Pipeline.Streams
{
    [Export(typeof(ILeafNode))]
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IAfterComposition))]
    public class StreamValueSink : AbstractPipelineNode, IHtValueLeaf, ISettingDefinitionSource, IAfterComposition
    {
        static PluginSelectionSettingDefinition ValuePlugin = new PluginSelectionSettingDefinition(PipelinePlugin.NodeFunc<IHtValueSinkToStream>, "ValueToStream", "StreamValueSink", "Values to output stream");
        static PluginSelectionSettingDefinition TransformPlugin = new PluginSelectionSettingDefinition(PipelinePlugin.NodeFunc<ISinkStreamTransform>, "StreamTransform", "StreamValueSink", "Transform", "Operation to perform on output stream, i.e. decompress");
        static PluginSelectionSettingDefinition SinkPlugin = new PluginSelectionSettingDefinition(PipelinePlugin.NodeFunc<ISinkStream>, "StreamSing", "StreamValueSink", "Final destination stream");

        private IPipelineNode GetPlugin(PluginState state, PluginSelectionSettingDefinition def)
        {
            return (def.FindMatchingItem(state.SettingValues[def.FullKey()]) as PluginSelectionItem)?.Plugin?.Cast<IPipelineNode>();
        }

        public StreamValueSink()
        : base(new MenuItem("DevTools.Pipeline.Output.StreamSink", "Custom output stream", typeof(StreamValueSink).FullName, "DevTools.Pipeline.Output"), new [] { ValuePlugin.FullKey(), TransformPlugin.FullKey(), SinkPlugin.FullKey()})
        {}

        public override string NodeDescription {
            get {
                var state = this.AssemblePluginState(PluginManager, SettingManager);
                return (GetPlugin(state, ValuePlugin)?.NodeDescription??string.Empty) + " -> "
                    + (GetPlugin(state, TransformPlugin)?.NodeDescription??string.Empty) + " -> "
                    + (GetPlugin(state, SinkPlugin)?.NodeDescription??string.Empty);
            }
        }

        public IEnumerable<ISettingDefinition> Definitions => new [] { ValuePlugin, TransformPlugin, SinkPlugin};

        private void SetDefaultValue(PluginSelectionSettingDefinition plugins)
        {
            var setting = SettingManager[plugins.FullKey()];
            if (setting.GetRaw()==string.Empty)
            {
                setting.SetRaw((plugins.GetAllItems().First() as PluginSelectionItem).Plugin.Name);
            }
        }
        public void AfterComposition()
        {
            ValuePlugin.SetPlugins(PluginManager.Plugins.Where(p => p.Cast<IHtValueSinkToStream>() != null).ToList(), SettingManager);
            SetDefaultValue(ValuePlugin);
            TransformPlugin.SetPlugins(PluginManager.Plugins.Where(p => p.Cast<ISinkStreamTransform>() != null).ToList(), SettingManager);
            SetDefaultValue(TransformPlugin);
            SinkPlugin.SetPlugins(PluginManager.Plugins.Where(p => p.Cast<ISinkStream>() != null).ToList(), SettingManager);
            SetDefaultValue(SinkPlugin);
        }

        public IHtValueSink GetHtValueSink(PluginState state)
        {
            return GetPlugin(state, ValuePlugin).Cast<IHtValueSinkToStream>().GetHtValueSink(state.NestedValues[ValuePlugin.FullKey()],
                GetPlugin(state, TransformPlugin).Cast<ISinkStreamTransform>().GetStream(state.NestedValues[TransformPlugin.FullKey()],
                    GetPlugin(state, SinkPlugin).Cast<ISinkStream>().GetStream(state.NestedValues[SinkPlugin.FullKey()])
                )
            );
        }
    }

    [Export(typeof(IPlugin))]
    public class CompressStream : HostedObjectBase, IPlugin, ISinkStreamTransform
    {
        [Import]
        IPluginManager PluginManager;

        [Import]
        ISettingManager SettingManager;
        public string Name => typeof(CompressStream).FullName;

        public string NodeDescription => "gzip";

        public PluginState GetPluginState(IDictionary<string, PluginState> visited = null)
        {
            return this.AssemblePluginState(PluginManager, SettingManager, visited);
        }

        public Stream GetStream(PluginState state, Stream outputStream)
        {
            return new GZipStream(outputStream, CompressionMode.Compress);
        }

        public void SetPluginState(PluginState state, ISet<string> visited = null)
        {
            this.DeployPluginState(state, PluginManager, SettingManager);
        }
        
    }}
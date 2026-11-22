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
    [Export(typeof(IRootNode))]
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IAfterComposition))]
    public class StreamValueSource : AbstractPipelineNode, IHtValueRoot, ISettingDefinitionSource, IAfterComposition
    {
        static PluginSelectionSettingDefinition SourcePlugin = new PluginSelectionSettingDefinition(PipelinePlugin.NodeFunc<IStreamSource>, "StreamSource", "StreamValueSource", "Input stream");
        static PluginSelectionSettingDefinition TransformPlugin = new PluginSelectionSettingDefinition(PipelinePlugin.NodeFunc<ISourceStreamTransform>, "StreamTransform", "StreamValueSource", "Transform", "Operation to perform on input stream, i.e. decompress");
        static PluginSelectionSettingDefinition ValuePlugin = new PluginSelectionSettingDefinition(PipelinePlugin.NodeFunc<IHtValueSourceFromStream>, "StreamToValue", "StreamValueSource", "Values from stream");

        private IPipelineNode GetPlugin(PluginState state, PluginSelectionSettingDefinition def)
        {
            return (def.FindMatchingItem(state.SettingValues[def.FullKey()]) as PluginSelectionItem)?.Plugin?.Cast<IPipelineNode>();
        }

        public StreamValueSource()
        : base(new MenuItem("DevTools.Pipeline.Input.StreamSource", "Custom stream", typeof(StreamValueSource).FullName, "DevTools.Pipeline.Input"), new [] { SourcePlugin.FullKey(), TransformPlugin.FullKey(), ValuePlugin.FullKey()})
        {}

        public override string NodeDescription {
            get {
                var state = this.AssemblePluginState(PluginManager, SettingManager);
                return (GetPlugin(state, SourcePlugin)?.NodeDescription??string.Empty) + " -> "
                    + (GetPlugin(state, TransformPlugin)?.NodeDescription??string.Empty) + " -> "
                    + (GetPlugin(state, ValuePlugin)?.NodeDescription??string.Empty);
            }
        }

        public IEnumerable<ISettingDefinition> Definitions => new [] { SourcePlugin, TransformPlugin, ValuePlugin};

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
            SourcePlugin.SetPlugins(PluginManager.Plugins.Where(p => p.Cast<IStreamSource>() != null).ToList(), SettingManager);
            SetDefaultValue(SourcePlugin);
            TransformPlugin.SetPlugins(PluginManager.Plugins.Where(p => p.Cast<ISourceStreamTransform>() != null).ToList(), SettingManager);
            SetDefaultValue(TransformPlugin);
            ValuePlugin.SetPlugins(PluginManager.Plugins.Where(p => p.Cast<IHtValueSourceFromStream>() != null).ToList(), SettingManager);
            SetDefaultValue(ValuePlugin);
        }

        public IHtValueSource GetHtValueSource(PluginState state)
        {
            return GetPlugin(state, ValuePlugin).Cast<IHtValueSourceFromStream>().GetHtValueSource(state.NestedValues[ValuePlugin.FullKey()],
                GetPlugin(state, TransformPlugin).Cast<ISourceStreamTransform>().GetStream(state.NestedValues[TransformPlugin.FullKey()],
                    GetPlugin(state, SourcePlugin).Cast<IStreamSource>().GetStream(state.NestedValues[SourcePlugin.FullKey()])
                )
            );
        }
    }

    [Export(typeof(IPlugin))]
    public class NullStreamTransform : HostedObjectBase, IPlugin, ISourceStreamTransform, ISinkStreamTransform
    {
        [Import]
        IPluginManager PluginManager;

        [Import]
        ISettingManager SettingManager;

        public string Name => typeof(NullStreamTransform).FullName;

        public string NodeDescription => "Unchanged stream";

        public PluginState GetPluginState(IDictionary<string, PluginState> visited = null)
        {
            return this.AssemblePluginState(PluginManager, SettingManager, visited);
        }

        public Stream GetStream(PluginState state, Stream inputStream)
        {
            return inputStream;
        }

        public void SetPluginState(PluginState state, ISet<string> visited = null)
        {
            this.DeployPluginState(state, PluginManager, SettingManager);
        }
    }

    [Export(typeof(IPlugin))]
    public class DecompressStream : HostedObjectBase, IPlugin, ISourceStreamTransform
    {
        [Import]
        IPluginManager PluginManager;

        [Import]
        ISettingManager SettingManager;
        public string Name => typeof(DecompressStream).FullName;

        public string NodeDescription => "gunzip";

        public PluginState GetPluginState(IDictionary<string, PluginState> visited = null)
        {
            return this.AssemblePluginState(PluginManager, SettingManager, visited);
        }

        public Stream GetStream(PluginState state, Stream inputStream)
        {
            return new GZipStream(inputStream, CompressionMode.Decompress);
        }

        public void SetPluginState(PluginState state, ISet<string> visited = null)
        {
            this.DeployPluginState(state, PluginManager, SettingManager);
        }
        
    }
}
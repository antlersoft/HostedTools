using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;
using com.antlersoft.HostedTools.Framework.Model;
using Newtonsoft.Json.Linq;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IAfterComposition))]
    public class PipelinePlugin : GridWorker, ISettingDefinitionSource, IAfterComposition
    {
        [ImportMany] public IEnumerable<IRootNode> Sources;
        [ImportMany] public IEnumerable<ILeafNode> Sinks;
        [ImportMany] public IEnumerable<IStemNode> Transforms;
        [Import] public IConditionBuilder ConditionBuilder;
        public static readonly PluginSelectionSettingDefinition Source =
            new PluginSelectionSettingDefinition(NodeFunc<IRootNode>, "Source", "Pipeline", "Data Source",
                "Click edit to change details of source");
        public static readonly PluginSelectionSettingDefinition Sink =
            new PluginSelectionSettingDefinition(NodeFunc<ILeafNode>, "Sink", "Pipeline", "Data Sink",
                "Click edit to change details of data destination");
        public static readonly PluginSelectionSettingDefinition Transform =
            new PluginSelectionSettingDefinition(NodeFunc<IStemNode>, "Transform", "Pipeline", "Data Transform",
                "Click edit to change details of data transform");
        public static readonly SimpleSettingDefinition GridOutput = new SimpleSettingDefinition("GridOutput", "Pipeline", "Display in Grid", "May want to leave unchecked for large data", typeof(bool), "false", false, 0);
        public static readonly ISettingDefinition IsSingleValued = new SimpleSettingDefinition("IsSingleValued", "Pipeline", "Is single values", "If checked, assumes data is a single JSON object instead of an array", typeof(bool), "false");

        public PipelinePlugin()
            : base(
                new[]
                {
                    new MenuItem("DevTools.Pipeline", "Data Pipeline", null, "DevTools"),
                    new MenuItem("DevTools.Pipeline.Plugin", "Run a pipeline", typeof (PipelinePlugin).FullName,
                        "DevTools.Pipeline"),
                    new MenuItem("DevTools.Pipeline.Input", "Input", null, "DevTools.Pipeline"),
                    new MenuItem("DevTools.Pipeline.Transform", "Transform", null, "DevTools.Pipeline"),
                    new MenuItem("DevTools.Pipeline.Output", "Output", null, "DevTools.Pipeline") 
                }, new[]
                {"Pipeline.Source", "Pipeline.LocalFilter", "Pipeline.Transform", "Pipeline.GridOutput", "Pipeline.Sink"}
                )
        {
            
        }

        public override void Perform(IWorkMonitor monitor)
        {
            ClearGrid(monitor);
            IHtValueRoot root = ((PluginSelectionItem)Source.FindMatchingItem(Source.Value<string>(SettingManager))).Plugin.Cast<IHtValueRoot>();
            IHtValueLeaf leaf = ((PluginSelectionItem)Sink.FindMatchingItem(Sink.Value<string>(SettingManager))).Plugin.Cast<IHtValueLeaf>();
            IHtValueStem stem = ((PluginSelectionItem)Transform.FindMatchingItem(Transform.Value<string>(SettingManager))).Plugin.Cast<IHtValueStem>();
            string filter = SettingManager["Pipeline.LocalFilter"].Get<string>();
            bool gridOutput = GridOutput.Value<bool>(SettingManager);
            CanBackground(monitor, "Pipe from "+root.NodeDescription+" to "+leaf.NodeDescription);

            var input = root.GetHtValueSource(root.GetPluginState());

            IEnumerable<IHtValue> rows = input.GetRows(monitor);
            if (! String.IsNullOrWhiteSpace(filter))
            {
                IHtExpression expression =
                    ConditionBuilder.ParseCondition(filter);
                if (expression != null)
                {
                    rows = rows.Where(r => expression.Evaluate(r).AsBool);
                }
            }
            IHtValueTransform transform = null;
            if (! (stem is NullTransform))
            {
                transform = stem.GetHtValueTransform(stem.GetPluginState());
                rows = transform.GetTransformed(rows, monitor);
            }
            if (gridOutput)
            {
                rows = rows.Select(r =>
                {
                    WriteRecord(monitor, r);
                    return r;
                });
            }
            var output = leaf.GetHtValueSink(leaf.GetPluginState());
            output.ReceiveRows(new MonitoredEnumerable<IHtValue>(rows, monitor), monitor);
            if (output.Cast<IDisposable>() is IDisposable disposable) {
                disposable.Dispose();
            }
            if (transform != null && transform.Cast<IDisposable>() is IDisposable disposable1) {
                disposable1.Dispose();
            }
            if (input.Cast<IDisposable>() is IDisposable disposable2) {
                disposable2.Dispose();
            }
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new[] {Source, Sink, Transform, GridOutput, IsSingleValued}; }
        }

        public void AfterComposition()
        {
            Source.SetPlugins(Sources.Select(s => s.Cast<IPlugin>()).Where(s => s!=null).ToList(), SettingManager);
            Sink.SetPlugins(Sinks.Select(s => s.Cast<IPlugin>()).Where(s => s != null).ToList(), SettingManager);
            Transform.SetPlugins(Transforms.Select(s => s.Cast<IPlugin>()).Where(s => s!=null).ToList(), SettingManager);
        }

        private static string NodeFunc<T>(IPlugin sourcePlugin) where T : class
        {
            T source = sourcePlugin.Cast<T>();
            if (source == null)
            {
                return sourcePlugin.Name;
            }
            return (source as IPipelineNode).NodeDescription;
        }

        class StreamSource : HostedObjectBase, IHtValueSource, IDisposable
        {
            Stream _str;
            Stream stream;
            JsonTextReader jr;
            IJsonFactory jsonFactory;
            bool isSingleValue;

            internal StreamSource(Stream str, IJsonFactory jf, bool isGz, bool isSv) {
                isSingleValue = isSv;
                _str = str;
                jsonFactory = jf;
                if (isGz) {
                    stream = new GZipStream(stream, CompressionMode.Decompress);
                } else {
                    stream = _str;
                }
                jr = new JsonTextReader(new StreamReader(stream));
            }
            public void Dispose()
            {
                (jr as IDisposable)?.Dispose();
                stream?.Dispose();
                if (_str != stream) {
                    _str?.Dispose();
                }
            }

            public IEnumerable<IHtValue> GetRows(IWorkMonitor monitor)
            {
                bool sentRow = false;
                using (StreamReader sw = new StreamReader(stream))
                using (var jr = new JsonTextReader(sw))
                {
                    var serializer = jsonFactory.GetSerializer();
                    if (isSingleValue)
                    {
                        yield return serializer.Deserialize<IHtValue>(jr);
                    }
                    else
                    {
                        // Skip array start
                        jr.Read();
                        jr.Read();
                        do
                        {
                            IHtValue value;
                            try
                            {
                                value = serializer.Deserialize<JsonHtValue>(jr);
                            }
                            catch (Exception)
                            {
                                break;
                            }
                            if (value == null || (! sentRow && value.IsEmpty))
                            {
                                break;
                            }
                            yield return value;
                            sentRow = true;
                        } while (jr.Read() && jr.TokenType != JsonToken.EndArray);
                    }
                }
            }
        }
        public static IHtValueSource FromJsonStream(Stream stream, IJsonFactory jsonFactory, bool isGzip, bool isSingleValue)
        {
            return new StreamSource(stream, jsonFactory, isGzip, isSingleValue);
        }

    }
}

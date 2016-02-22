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

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IAfterComposition))]
    public class PipelinePlugin : GridWorker, ISettingDefinitionSource, IAfterComposition
    {
        [ImportMany] public IEnumerable<IHtValueSource> Sources;
        [ImportMany] public IEnumerable<IHtValueSink> Sinks;
        [ImportMany] public IEnumerable<IHtValueTransform> Transforms;
        [Import] public IConditionBuilder ConditionBuilder;
        public static readonly PluginSelectionSettingDefinition Source =
            new PluginSelectionSettingDefinition(SourceFunc, "Source", "Pipeline", "Data Source",
                "Click edit to change details of source");
        public static readonly PluginSelectionSettingDefinition Sink =
            new PluginSelectionSettingDefinition(SinkFunc, "Sink", "Pipeline", "Data Sink",
                "Click edit to change details of data destination");
        public static readonly PluginSelectionSettingDefinition Transform =
            new PluginSelectionSettingDefinition(TransformFunc, "Transform", "Pipeline", "Data Transform",
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
                {"Pipeline.Source", "DynamoQuery.LocalFilter", "Pipeline.Transform", "Pipeline.GridOutput", "Pipeline.Sink"}
                )
        {
            
        }

        public override void Perform(IWorkMonitor monitor)
        {
            ClearGrid(monitor);
            IHtValueSource input = ((PluginSelectionItem)Source.FindMatchingItem(Source.Value<string>(SettingManager))).Plugin.Cast<IHtValueSource>();
            IHtValueSink output = ((PluginSelectionItem)Sink.FindMatchingItem(Sink.Value<string>(SettingManager))).Plugin.Cast<IHtValueSink>();
            IHtValueTransform transform = ((PluginSelectionItem)Transform.FindMatchingItem(Transform.Value<string>(SettingManager))).Plugin.Cast<IHtValueTransform>();
            string filter = SettingManager["DynamoQuery.LocalFilter"].Get<string>();
            bool gridOutput = GridOutput.Value<bool>(SettingManager);
            CanBackground(monitor, "Pipe from "+input.SourceDescription+" to "+output.SinkDescription);
            IEnumerable<IHtValue> rows = input.GetRows();
            if (! String.IsNullOrWhiteSpace(filter))
            {
                IHtExpression expression =
                    ConditionBuilder.ParseCondition(filter);
                if (expression != null)
                {
                    rows = rows.Where(r => expression.Evaluate(r).AsBool);
                }
            }
            if (! (transform is NullTransform))
            {
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
            output.ReceiveRows(new MonitoredEnumerable<IHtValue>(rows, monitor), monitor);
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

        private static string SourceFunc(IPlugin sourcePlugin)
        {
            IHtValueSource source = sourcePlugin.Cast<IHtValueSource>();
            if (source == null)
            {
                return sourcePlugin.Name;
            }
            return source.SourceDescription;
        }

        private static string SinkFunc(IPlugin sinkPlugin)
        {
            IHtValueSink sink = sinkPlugin.Cast<IHtValueSink>();
            if (sink == null)
            {
                return sinkPlugin.Name;
            }
            return sink.SinkDescription;
        }

        private static string TransformFunc(IPlugin plugin)
        {
            IHtValueTransform transform = plugin.Cast<IHtValueTransform>();
            if (transform == null)
            {
                return plugin.Name;
            }
            return transform.TransformDescription;
        }

        public static IEnumerable<IHtValue> FromJsonStream(Stream stream, IJsonFactory jsonFactory, bool isGzip, bool isSingleValue)
        {
            if (isGzip)
            {
                stream = new GZipStream(stream, CompressionMode.Decompress);
            }
            try
            {
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
                            yield return value;
                        } while (jr.Read() && jr.TokenType != JsonToken.EndArray);
                    }
                }
            }
            finally
            {
                stream.Dispose();
            }
        }

    }
}

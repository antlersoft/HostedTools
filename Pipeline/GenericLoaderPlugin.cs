
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    public class Settings : ISettingDefinitionSource
    {
        public static readonly ISettingDefinition LoadFile = new PathSettingDefinition("LoadFile", "GenericLoader", "Data File", false, false, "json file|*.json|gzip'd json|*.gz", "File containing json data to load");
        public static readonly ISettingDefinition TableToLoad = new SimpleSettingDefinition("TableToLoad", "GenericLoader", "Table to load into");
        public static readonly ISettingDefinition DataIsGzip = new SimpleSettingDefinition("DataIsGzip", "GenericLoader", "Data is GZipped", "If checked, assumes data is compressed in gzip format", typeof(bool), "false");
        public static readonly ISettingDefinition FullLoad = new SimpleSettingDefinition("FullLoad", "GenericLoader", "Replace all columns", "If checked, will replace all columns in table with values from file; otherwise, will only update columns specified in file", typeof(bool), "true");
        public static readonly ISettingDefinition GZipData = new SimpleSettingDefinition("GZipData", "GenericLoader", "GZip Data", "If checked, will gzip data", typeof(bool), "false");
        public static readonly ISettingDefinition TableToDump = new SimpleSettingDefinition("TableToDump", "GenericLoader", "Table to dump");
        public static readonly ISettingDefinition OutputFile = new PathSettingDefinition("OutputFile", "GenericLoader", "Output File", true, false, "json file|*.json|gzip'd json|*.gz", "File where json data will be written");

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new[] { LoadFile, TableToLoad, FullLoad, TableToDump, OutputFile, GZipData, DataIsGzip }; }
        }
    }

    [Export(typeof (IRootNode))]
    public class JsonFileSource : AbstractPipelineNode, IHtValueRoot
    {
        [Import]
        public IJsonFactory JsonFactory;

        public JsonFileSource()
            : base(new MenuItem("DevTools.Pipeline.Input.JsonFile", "json file", typeof(JsonFileSource).FullName, "DevTools.Pipeline.Input"), new[] { Settings.LoadFile.FullKey(), Settings.DataIsGzip.FullKey(), PipelinePlugin.IsSingleValued.FullKey()})
        {
            
        }

        public IHtValueSource GetHtValueSource(PluginState state)
        {
            string loadFile = state.SettingValues[Settings.LoadFile.FullKey()];
            var stream=new FileStream(loadFile, FileMode.Open, FileAccess.Read);
            return PipelinePlugin.FromJsonStream(stream, JsonFactory,
                new JsonHtValue(state.SettingValues[Settings.DataIsGzip.FullKey()]).AsBool,
                new JsonHtValue(state.SettingValues[PipelinePlugin.IsSingleValued.FullKey()]).AsBool);
        }

        public override string NodeDescription
        {
            get { return "Read json from " + Settings.LoadFile.Value<string>(SettingManager) + (Settings.DataIsGzip.Value<bool>(SettingManager)?" gzip'd":string.Empty); }
        }
    }

    [Export(typeof(ILeafNode))]
    public class JsonFileSink : AbstractPipelineNode, IHtValueLeaf
    {
        [Import]
        public IJsonFactory JsonFactory;

        public JsonFileSink()
            : base(new MenuItem("DevTools.Pipeline.Output.JsonFile", "json file", typeof(JsonFileSink).FullName, "DevTools.Pipeline.Output"), new[] { Settings.OutputFile.FullKey(), Settings.GZipData.FullKey() })
        {

        }

        public override string NodeDescription
        {
            get { return "Write json to " + Settings.OutputFile.Value<string>(SettingManager) + (Settings.GZipData.Value<bool>(SettingManager) ? " gzip'd" : String.Empty); }
        }

        public IHtValueSink GetHtValueSink(PluginState state)
        {
            string outputPath = state.SettingValues[Settings.OutputFile.FullKey()];
            bool useGzip = new JsonHtValue(state.SettingValues[Settings.GZipData.FullKey()]).AsBool;
            return new Sink(JsonFactory, outputPath, useGzip);
        }

        internal class Sink : HostedObjectBase, IHtValueSink, IDisposable {
            private IJsonFactory jsonFactory;
            private JsonTextWriter jr;

            internal Sink(IJsonFactory jf, string outputPath, bool useGzip) {
                jsonFactory = jf;
                StreamWriter sr = useGzip ? new StreamWriter(new GZipStream(new FileStream(outputPath, FileMode.Create), CompressionMode.Compress))
                    : new StreamWriter(outputPath);
                jr = new JsonTextWriter(sr);
            }
            public void Dispose()
            {
                (jr as IDisposable).Dispose();
            }

            public void ReceiveRows(IEnumerable<IHtValue> rows, IWorkMonitor monitor)
            {
                ICancelableMonitor cancelable = monitor.Cast<ICancelableMonitor> ();
                        JsonSerializer serializer = jsonFactory.GetSerializer(true);
                jr.WriteStartArray();
                foreach (var v in rows)
                {
                    if (cancelable!=null && cancelable.IsCanceled)
                    {
                        break;
                    }
                    serializer.Serialize(jr, v);
                }
                jr.WriteEndArray();
            }
        }
    }
}

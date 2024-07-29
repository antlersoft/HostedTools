
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

    [Export(typeof (IHtValueSource))]
    public class JsonFileSource : EditOnlyPlugin, IHtValueSource
    {
        [Import]
        public IJsonFactory JsonFactory;
        private FileStream _stream;

        public JsonFileSource()
            : base(new MenuItem("DevTools.Pipeline.Input.JsonFile", "json file", typeof(JsonFileSource).FullName, "DevTools.Pipeline.Input"), new[] { Settings.LoadFile.FullKey(), Settings.DataIsGzip.FullKey(), PipelinePlugin.IsSingleValued.FullKey()})
        {
            
        }

        public IEnumerable<IHtValue> GetRows()
        {
            string loadFile = Settings.LoadFile.Value<string>(SettingManager);
            if (_stream != null) {
                _stream.Dispose();
                _stream = null;
            }
            _stream=new FileStream(loadFile, FileMode.Open, FileAccess.Read);
            return PipelinePlugin.FromJsonStream(_stream, JsonFactory,
                Settings.DataIsGzip.Value<bool>(SettingManager),
                PipelinePlugin.IsSingleValued.Value<bool>(SettingManager));
        }

        public string SourceDescription
        {
            get { return "Read json from " + Settings.LoadFile.Value<string>(SettingManager) + (Settings.DataIsGzip.Value<bool>(SettingManager)?" gzip'd":string.Empty); }
        }
    }

    [Export(typeof(IHtValueSink))]
    public class JsonFileSink : EditOnlyPlugin, IHtValueSink
    {
        [Import]
        public IJsonFactory JsonFactory;

        public JsonFileSink()
            : base(new MenuItem("DevTools.Pipeline.Output.JsonFile", "json file", typeof(JsonFileSink).FullName, "DevTools.Pipeline.Output"), new[] { Settings.OutputFile.FullKey(), Settings.GZipData.FullKey() })
        {

        }

        public string SinkDescription
        {
            get { return "Write json to " + Settings.OutputFile.Value<string>(SettingManager) + (Settings.GZipData.Value<bool>(SettingManager) ? " gzip'd" : String.Empty); }
        }

        public void ReceiveRows(IEnumerable<IHtValue> rows, IWorkMonitor monitor)
        {
            string outputPath = Settings.OutputFile.Value<string>(SettingManager);
			ICancelableMonitor cancelable = monitor.Cast<ICancelableMonitor> ();
            using (var sr = Settings.GZipData.Value<bool>(SettingManager) ?
                new StreamWriter(new GZipStream(new FileStream(outputPath, FileMode.Create), CompressionMode.Compress))
                : new StreamWriter(outputPath))
            {
                using (var jr = new JsonTextWriter(sr))
                {
                    JsonSerializer serializer = JsonFactory.GetSerializer(true);
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
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IPlugin))]
    [Export(typeof(IStemNode))]    
    public class TempFileTransform : IPlugin, IHtValueStem, IStemNode
    {
        [Import]
        public IJsonFactory JsonFactory { get; set; }

        public string NodeDescription => "Output intermediate results to a temporary file";

        public string Name => GetType().FullName;

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return new Transform(JsonFactory);
        }

        public PluginState GetPluginState(ISet<string> visited = null)
        {
            PluginState result = new PluginState();
            result.PluginName = Name;
            result.NestedValues = new Dictionary<string, PluginState>();
            result.SettingValues = new Dictionary<string, string>();

            return result;
        }

        internal IHtValueTransform GetTempFileTransform() {
            return new Transform(JsonFactory);
        }

        class Transform : IHtValueTransform, IDisposable {
            private string _tempFilePath;
            private IHtValueSource _source;
            private IJsonFactory _jsonFactory;

            internal Transform(IJsonFactory jsonFactory)
            {
                _jsonFactory = jsonFactory;
            }

            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
            {
                _tempFilePath = Path.GetTempFileName();

                using (var sink = new JsonFileSink.Sink(_jsonFactory, _tempFilePath, true)) {
                    sink.ReceiveRows(input, monitor);
                }
                monitor.Writer.WriteLine("Finished writing to temp file " + _tempFilePath);
                //GC.Collect();

                _source = PipelinePlugin.FromJsonStream(new FileStream(_tempFilePath, FileMode.Open, FileAccess.Read), _jsonFactory, true, false);
                return _source.GetRows(monitor);
            }

            public void Dispose()
            {
                if (_source != null) {
                    if (_source is IDisposable disposable) {
                        disposable.Dispose();
                    }
                    _source = null;
                }
                if (_tempFilePath != null)
                {
                    File.Delete(_tempFilePath);
                    _tempFilePath = null;
                }
            }

            public T Cast<T>(bool fromAggregated = false) where T : class
            {
                return this as T;
            }
        }

        public void SetPluginState(PluginState state, ISet<string> visited = null)
        {
        }
    }
}
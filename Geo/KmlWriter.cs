using System.ComponentModel.Composition;
using System.Web;
using com.antlersoft.HostedTools.Pipeline;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Interface.Geography;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;
using System.Net;
using com.antlersoft.HostedTools.Framework.Model;


namespace com.antlersoft.HostedTools.Geography
{
    [Export(typeof(ILeafNode))]
    [Export(typeof(ISettingDefinitionSource))]
    public class KmlSink : AbstractPipelineNode, ISettingDefinitionSource, IAfterComposition, IHtValueLeaf
    {
        [Import]
        public IWorkMonitorSource? MonitorSource { get; set; }
        public static ISettingDefinition KmlFile = new PathSettingDefinition("KmlFile", "Shapefile", "Output Kml File", true, false, "kml file|*.kml");
        public static ISettingDefinition WriteToFile = new SimpleSettingDefinition("KmlWriteFile", "Shapefile", "Write to File", null, typeof(bool), "false", false, 0);
        public static ISettingDefinition ServeToWeb = new SimpleSettingDefinition("ServeToWeb", "Shapefile", "Serve to web", null, typeof(bool), "false", false, 0);
        public static ISettingDefinition WebPort = new SimpleSettingDefinition("WebPort", "Shapefile", "Web port", "KML Web Server will listen on this port", typeof(int), "17571");
        public static ISettingDefinition MetadataField = new SimpleSettingDefinition("MetadataField", "Shapefile", "Metadata Field Name", "Name of field in shapefile metadata to use to label bounding box", typeof(string));


        private ICancelableMonitor? _cancelableMonitor;
        private KmlWriter.KmlWebServer? _webServer;

        public KmlSink()
            : base(
                new MenuItem("DevTools.Pipeline.Output.Kml", "KmlWriter", typeof (KmlSink).FullName,
                    "DevTools.Pipeline.Output"), new[] {MetadataField.FullKey(), ServeToWeb.FullKey(), WebPort.FullKey(), WriteToFile.FullKey(), KmlFile.FullKey()})
        {
            
        }

        class Sink : HostedObjectBase, IHtValueSink {
            bool _writeToFile;
            string _metaDataField;
            string _kmlFile;
            KmlWriter.KmlWebServer? _server;

            internal Sink(bool writeToFile, string kmlFile, string metaDataField, KmlWriter.KmlWebServer? server) {
                _writeToFile=writeToFile;
                _kmlFile = kmlFile;
                _metaDataField = metaDataField;
                _server = server;
            }

            public void ReceiveRows(IEnumerable<IHtValue> rows, IWorkMonitor monitor)
            {
                var setting = new JsonFactory().GetSettings();
                KmlWriter? fileWriter = _writeToFile ? new KmlWriter(_kmlFile) : null;
                StringWriter? sw = _server == null ? null : new StringWriter();
                KmlWriter? webWriter = sw!=null ? new KmlWriter(sw) : null;
                try
                {
                    foreach (var row in rows)
                    {
                        if (monitor.Cast<ICancelableMonitor>()?.IsCanceled??false)
                        {
                            break;
                        }

                        var geoJson=JsonConvert.DeserializeObject<GeoJson>(JsonConvert.SerializeObject(row, setting), setting);
                        if (geoJson.type == "Feature" && _metaDataField!=null && geoJson.id==null && geoJson.properties!=null) {
                            string id;
                            var idObject = geoJson.properties[_metaDataField];
                            if (idObject!=null && !idObject.IsEmpty) {
                                geoJson.id = idObject.AsString;
                            }
                        }
                        if (fileWriter != null) {
                            fileWriter.WriteGeoJson(geoJson);
                        }
                        if (webWriter != null) {
                            webWriter.WriteGeoJson(geoJson);
                        }
                    }
                }
                finally
                {
                    if (webWriter!=null) {
                        webWriter.Close();
                        if (_server != null && sw != null) {
                            _server.PushContent(sw.ToString());
                        }
                    }
                    if (fileWriter != null) {
                        fileWriter.Close();
                    }
                }
            }
        }

        /// <summary>
        /// If at this time you get a ICancelableMonitor from IWorkMonitorHolder, it should be canceled when UI
        /// is shut down
        /// </summary>
        public void AfterComposition()
        {
            _cancelableMonitor = MonitorSource?.GetMonitor()?.Cast<ICancelableMonitor>();
        }

        public IHtValueSink GetHtValueSink(PluginState state)
        {
            bool writeToWeb = (bool)Convert.ChangeType(state.SettingValues[ServeToWeb.FullKey()], typeof(bool));
            if (writeToWeb) {
                if (_webServer == null) {
                    int port = (int)Convert.ChangeType(state.SettingValues[WebPort.FullKey()], typeof(int));
                    CancellationToken token = _cancelableMonitor?.Cancellation ?? CancellationToken.None;
                    _webServer = new KmlWriter.KmlWebServer(port, token);
                    _webServer.Start();
                }
            }
            return new Sink((bool)Convert.ChangeType(state.SettingValues[WriteToFile.FullKey()], typeof(bool)),
                    state.SettingValues[KmlFile.FullKey()], state.SettingValues[MetadataField.FullKey()], writeToWeb ? _webServer : null);
        }

        public override string NodeDescription
        {
            get { return "Write geometry to Kml file "+KmlFile.Value<string>(SettingManager); }
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new [] { KmlFile, MetadataField, WriteToFile, ServeToWeb, WebPort }; }
        }
    }

    class KmlWriter : IDisposable
    {
        TextWriter? _sw;

        internal KmlWriter(TextWriter sw) {
            _sw = sw;
            _sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<kml xmlns=\"http://www.opengis.net/kml/2.2\"><Document><Style id=\"scaledIcon1\"><IconStyle><scale>0.3</scale></IconStyle></Style><Style id=\"scaledIcon2\"><IconStyle><scale>0.3</scale><color>ff0000ff</color></IconStyle></Style><Style id=\"scaledIcon3\"><IconStyle><scale>0.3</scale><color>FFC878F0</color></IconStyle></Style><Style id=\"boundingBox\"><LineStyle><color>ff0000ff</color></LineStyle></Style><Folder>\r\n");
        }

        internal KmlWriter(string path)
        : this (new StreamWriter(path))
        {

        }

        internal void Close()
        {
            if (_sw != null)
            {
                _sw.WriteLine("</Folder></Document></kml>");
                _sw.Close();
                _sw = null;
            }
        }

        internal void WriteBoundingBox(IGeoBoundingBox bb, string? descr = null)
        {
            _sw.WriteLine("<Placemark><styleUrl>#boundingBox</styleUrl><name>{4}</name><LineString><altitudeMode>clampToGround</altitudeMode><coordinates>{0},{1} {2},{1} {2},{3} {0},{3} {0},{1}</coordinates></LineString></Placemark>", bb.West, bb.North, bb.East, bb.South, HttpUtility.HtmlEncode(descr ?? string.Empty));
        }

        internal void WritePoly(string description, GeoJsonGeometry g)
        {
            _sw.Write($"<Placemark><description><LineString><altitudeMode>clampToGround</altitudeMode><coordinates>");
            foreach (var coord in g.coordinates.AsArrayElements) {
                _sw.Write("{0},{1} ", coord[0].AsDouble, coord[1].AsDouble);               
            }
            _sw.WriteLine("</coordinates></LineString></Placemark>");
        }

        internal void WritePoint(string description, IGeoPoint gp)
        {
            _sw.WriteLine("<Placemark><description>{0}</description><Point><coordinates>{1},{2},0</coordinates></Point></Placemark>", HttpUtility.HtmlEncode(description), gp.Longitude, gp.Latitude);
        }

        internal void WriteGeometry(string description, GeoJsonGeometry g) {
            if (g.type == "GeometryCollection") {
                if (g.geometries != null) {
                    foreach (var geo in g.geometries) {
                        WriteGeometry(description, geo);
                    }
                }
            }
            if (g.coordinates == null) 
                return;
            if (g.type == "Point") {
                _sw.WriteLine("<Placemark><description>{0}</description><Point><coordinates>{1},{2},0</coordinates></Point></Placemark>", HttpUtility.HtmlEncode(description), g.coordinates[0].AsDouble, g.coordinates[1]);            
            } else if (g.type == "LineString" || g.type == "Polygon") {
                WritePoly(description, g);
            } else {
                foreach (var subCoordinates in g.coordinates.AsArrayElements) {
                    var geo = new GeoJsonGeometry();
                    geo.type = g.type=="MultiLineString" ? "LineString" : "Polygon";
                    geo.coordinates = new JsonHtValue(subCoordinates);
                    WriteGeometry(description, geo);
                }
            }
        }

        public void WriteGeoJsonFeature(GeoJsonFeature feature) {
            string description = string.Empty;
            if (feature.id!=null) {
                description = feature.id;
            } else {
                if (feature.properties != null) {
                    if (feature.properties.IsString) {
                        description = feature.properties.AsString;
                    } else {
                        foreach (var pair in feature.properties.AsDictionaryElements) {
                            if (pair.Key == "description") {
                                description=pair.Value.AsString;
                                break;
                            } else if (pair.Value.IsString) {
                                description=pair.Value.AsString;
                            }
                        }
                    }
                }
            }
            if (feature.geometry?.type == "Point") {
                WritePoint(description, feature);
            } else {
                WriteGeometry(description, feature.geometry);
            }
        }

        public void WriteGeoJson(GeoJson g) {
            if (g.type == "FeatureCollection") {
                if (g.features != null) {
                    foreach (var feature in g.features) {
                        WriteGeoJsonFeature(feature);
                    }
                }
            } else if (g.type == "Feature") {
                GeoJsonFeature feature = new GeoJsonFeature();
                feature.type = g.type;
                feature.geometry = g.geometry;
                feature.id = g.id;
                feature.properties = g.properties;
                WriteGeoJsonFeature(feature);
            } else {
                // It is a geometry type
                GeoJsonGeometry geometry=new GeoJsonGeometry();
                geometry.coordinates = g.coordinates;
                geometry.type = g.type;
                geometry.geometries = g.geometries;
                WriteGeometry(string.Empty, geometry);
            }
        }

        public void Dispose()
        {
            Close();
        }

        internal class KmlWebServer : IDisposable
        {
            HttpListener? _listener;
            int _port;
            CancellationToken _token;
            private string? _content;

            internal KmlWebServer(int port, CancellationToken token) {
                _port = port;
                _token = token;
            }

            internal void PushContent(string content) {
                _content = content;
            }

            internal async void Start() {
                try {
                    _listener = new HttpListener();
                    _listener.Prefixes.Add($"http://+:{_port}/");
                    _listener.Start();

                    while (! _token.IsCancellationRequested) {
                        var context =await _listener.GetContextAsync().WaitAsync(_token);
                        if (! _token.IsCancellationRequested) {
                            await ProcessContext(context);
                        }
                    }
                    Dispose();
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                    Dispose();
                }
            }

            private async Task ProcessContext(HttpListenerContext context) {
                var response = context.Response;
                if (_content != null) {
                    response.StatusCode = 200;
                    response.ContentType = "application/vnd.google-earth.kml+xm";
                    response.ContentLength64 = _content.Length;
                    using (StreamWriter writer = new StreamWriter(response.OutputStream)) {
                        await writer.WriteAsync(_content);
                    }
                } else {
                    response.StatusCode = 403;
                }
            }

            public void Dispose()
            {
                if (_listener != null) {
                    _listener.Close();
                    _listener = null;
                }
            }
        }
    }

}

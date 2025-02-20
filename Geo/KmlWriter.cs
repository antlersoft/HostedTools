using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
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


namespace com.antlersoft.HostedTools.Geography
{
    [Export(typeof(IHtValueSink))]
    [Export(typeof(ISettingDefinitionSource))]
    public class KmlSink : EditOnlyPlugin, IHtValueSink, ISettingDefinitionSource
    {
        public static ISettingDefinition KmlFile = new PathSettingDefinition("KmlFile", "Shapefile", "Output Kml File", true, false, "kml file|*.kml");
        public static ISettingDefinition MetadataField = new SimpleSettingDefinition("MetadataField", "Shapefile", "Metadata Field Name", "Name of field in shapefile metadata to use to label bounding box", typeof(string));

        public KmlSink()
            : base(
                new MenuItem("DevTools.Pipeline.Output.Kml", "KmlWriter", typeof (KmlSink).FullName,
                    "DevTools.Pipeline.Output"), new[] {KmlFile.FullKey(), MetadataField.FullKey()})
        {
            
        }

        public void ReceiveRows(IEnumerable<IHtValue> rows, IWorkMonitor monitor)
        {
            var setting = new JsonFactory().GetSettings();
            using (KmlWriter writer = new KmlWriter(KmlFile.Value<string>(SettingManager)))
            {
                foreach (var row in rows)
                {
                    if (monitor.Cast<ICancelableMonitor>()?.IsCanceled??false)
                    {
                        break;
                    }

                    JsonConvert.DeserializeObject<GeoJson>(JsonConvert.SerializeObject(row, setting));
                }
            }
        }

        public string SinkDescription
        {
            get { return "Write geometry to Kml file "+KmlFile.Value<string>(SettingManager); }
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new [] { KmlFile, MetadataField }; }
        }
    }

    class KmlWriter : IDisposable
    {
        StreamWriter? _sw;
        internal KmlWriter(string path)
        {
            _sw = new StreamWriter(path);
            _sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<kml xmlns=\"http://www.opengis.net/kml/2.2\"><Document><Style id=\"scaledIcon1\"><IconStyle><scale>0.3</scale></IconStyle></Style><Style id=\"scaledIcon2\"><IconStyle><scale>0.3</scale><color>ff0000ff</color></IconStyle></Style><Style id=\"scaledIcon3\"><IconStyle><scale>0.3</scale><color>FFC878F0</color></IconStyle></Style><Style id=\"boundingBox\"><LineStyle><color>ff0000ff</color></LineStyle></Style><Folder>\r\n");
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
                    geo.coordinates = subCoordinates;
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
    }

}

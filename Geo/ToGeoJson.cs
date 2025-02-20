using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.Geography
{
    /// <summary>
    /// Inspired by MapBox toGeoJson module
    /// </summary>
    internal class ToGeoJson
    {
        static readonly string[] geotypes = { "Polygon", "LineString", "Point", "Track" };

        private XmlDocument _document;
        private XmlNamespaceManager _namespaceManager;

        static string NormalizeSpace(string s)
        {
            StringBuilder sb = new StringBuilder();
            bool lastWhite = false;
            foreach (char ch in s)
            {
                if (Char.IsWhiteSpace(ch))
                {
                    if (! lastWhite)
                    {
                        sb.Append(' ');
                    }
                    lastWhite = true;
                }
                else
                {
                    sb.Append(ch);
                    lastWhite = false;
                }
            }
            return sb.ToString();
        }

        XmlNodeList Get(XmlNode doc, string key)
        {
            return doc.SelectNodes("descendant::km:"+key, _namespaceManager);
        }

        XmlNode Get1(XmlNode doc, string key)
        {
            XmlNodeList nodes = Get(doc, key);
            if (nodes != null && nodes.Count > 0)
            {
                return nodes[0];
            }
            return null;
        }

        static string Attr(XmlNode node, string attrName)
        {
            return node.Attributes[attrName].Value;
        }

        static double parseFloat(string f)
        {
            double result;
            if (double.TryParse(f, out result))
            {
                return result;
            }
            return Double.NaN;
        }

        static List<double> numarray(string[] x)
        {
            return x.Select(parseFloat).ToList();
        }

        static IHtValue coord(string s)
        {
            string[] coords = NormalizeSpace(s).Trim().Split(' ');
            IHtValue result = new JsonHtValue();
            int i = 0;
            foreach (string c in coords)
            {
                result[i++] = coord1(c);
            }
            return result;
        }

        static IHtValue coord1(string s)
        {
            var doubles = numarray(NormalizeSpace(s).Replace(" ", String.Empty).Split(','));
            int i = 0;
            IHtValue result = new JsonHtValue();
            foreach (var d in doubles)
            {
                result[i++] = new JsonHtValue(d);
            }
            return result;
        }

        static string nodeVal(XmlNode n)
        {
            if (n == null)
            {
                return string.Empty;
            }
            return n.InnerText;
        }

        ToGeoJson(XmlDocument doc)
        {
            _document = doc;
            _namespaceManager = new XmlNamespaceManager(doc.NameTable);
            _namespaceManager.AddNamespace("km", _document.DocumentElement.NamespaceURI);
        }

        private GeoJson ConvertKml(string geometryType = null)
        {
            GeoJson gj = new GeoJson() { features = new List<GeoJsonFeature>(), type = "FeatureCollection" };
            // Hashed styles in order to match features
            XmlNodeList placemarks = Get(_document, "Placemark");
            int count = placemarks.Count;
            for (int i = 0; i < count; i++)
            {
                var pm = GetPlacemark(placemarks[i], geometryType);
                if (pm != null)
                {
                    gj.features.Add(pm);
                }
            }
            return gj;
        }

        internal static GeoJson ConvertKml(XmlDocument doc, string? geometryType = null)
        {
            return new ToGeoJson(doc).ConvertKml(geometryType);
        }

        internal static GeoJson ConvertKml(string xml, string geometryType)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return ConvertKml(doc, geometryType);
        }

        internal static GeoJson ConvertKml(Stream str, string? geometryType = null)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(str);
            return ConvertKml(doc, geometryType);
        }

        private static bool MatchingType(string geo, string toMatch, string geometryType)
        {
            return geo == toMatch && (geometryType == null || geo == geometryType);
        }

        GeoJsonGeometry GetGeometry(XmlNode root, string geometryType)
        {
            List<GeoJsonGeometry> geoms = new List<GeoJsonGeometry>();
            var multiNode = Get1(root, "MultiGeometry");
            if (multiNode != null)
            {
                return GetGeometry(multiNode, geometryType);
            }
            multiNode = Get1(root, "MultiTrack");
            if (multiNode != null)
            {
                return GetGeometry(multiNode, geometryType);
            }
            foreach (string geo in geotypes)
            {
                var geomNodes = Get(root, geo);
                if (geomNodes != null)
                {
                    int count = geomNodes.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var geomNode = geomNodes[i];
                        if (MatchingType(geo, "Point", geometryType))
                        {
                            geoms.Add(new GeoJsonGeometry() {type=geo, coordinates=coord1(nodeVal(Get1(geomNode, "coordinates")))});
                        }
                        else if (MatchingType(geo, "LineString", geometryType))
                        {
                            geoms.Add(new GeoJsonGeometry() { type = geo, coordinates = coord(nodeVal(Get1(geomNode, "coordinates")))});
                        }
                        else if (MatchingType(geo, "Polygon", geometryType))
                        {
                            var rings = Get(geomNode, "LinearRing");
                            IHtValue coords = new JsonHtValue();
                            for (int r = 0; r<rings.Count; r++)
                            {
                                coords[r] = coord(nodeVal(Get1(rings[r], "coordinates")));
                            }
                            geoms.Add(new GeoJsonGeometry() { type = geo, coordinates = coords});
                        }
                    }
                }
            }
            if (geoms.Count == 1)
            {
                return geoms[0];
            }
            return new GeoJsonGeometry() {type = "GeometryCollection", geometries = geoms};
        }

        void AddProperty(IHtValue properties, XmlNode root, string key)
        {
            string val = nodeVal(Get1(root, key));
            if (!string.IsNullOrEmpty(val))
            {
                properties[key] = new JsonHtValue(val);
            }
        }

        GeoJsonFeature GetPlacemark(XmlNode root, string geometryType)
        {
            var geometry = GetGeometry(root, geometryType);
            if (geometry.geometries != null && geometry.geometries.Count == 0)
            {
                return null;
            }
            JsonHtValue properties = new JsonHtValue();
            AddProperty(properties, root, "name");
            AddProperty(properties, root, "description");
            AddProperty(properties, root, "styleUrl");
            var timespan = Get1(root, "TimeSpan");
            if (timespan != null)
            {
                AddProperty(properties, timespan, "begin");
                AddProperty(properties, timespan, "end");
            }
            XmlNode extendedData = Get1(root, "ExtendedData");
            if (extendedData != null)
            {
                var datas = Get(extendedData, "SimpleData");
                int l = datas.Count;
                for (int i = 0; i < l; i++)
                {
                    var sd = datas[i];
                    properties[Attr(sd, "name")] = new JsonHtValue(nodeVal(Get1(sd, "value")));
                }
                var simpleDatas = Get(extendedData, "SimpleData");
                l = simpleDatas.Count;
                for (int i = 0; i < l; i++)
                {
                    var sd = simpleDatas[i];
                    properties[Attr(sd, "name")] = new JsonHtValue(nodeVal(sd));
                }
            }
            return new GeoJsonFeature {type = "Feature", geometry = geometry, properties = properties};
        }
    }
}

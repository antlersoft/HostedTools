using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Pipeline;
using com.antlersoft.HostedTools.Pipeline.Extensions;
using com.antlersoft.HostedTools.Serialization;
using NetTopologySuite.IO.Esri;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.antlersoft.HostedTools.Geography {

    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IRootNode))]
    public class ShapefileSource : SimpleWorker, ISettingDefinitionSource, IHtValueRoot {
        [Import]
        IPluginManager? PluginManager;

        static ISettingDefinition ShapefilePath = new PathSettingDefinition("ShapefilePath", "Geo", "Shapefile path", false, false, "shp|*.shp", "Shapefile to open");
        static ISettingDefinition IDAttribute = new SimpleSettingDefinition("IDAttribute", "Geo", "ID Attribute", "This attribute will be used as id of GeoJsonFeature");

        public ShapefileSource()
        : base(new MenuItem("DevTools.Pipeline.Input.Shapefile", "Shapefile", typeof(ShapefileSource).FullName, "DevTools.Pipeline.Input"), new [] {ShapefilePath.FullKey(), IDAttribute.FullKey()}) {
            
        }

        public IEnumerable<ISettingDefinition> Definitions => new[] { ShapefilePath, IDAttribute };

        public string NodeDescription => $"Read shapefile "+ShapefilePath.Value<string>(SettingManager);
        public PluginState GetPluginState(ISet<string>? visited = null)
        {
            return this.AssemblePluginState(PluginManager, SettingManager, visited);
        }

        public override void Perform(IWorkMonitor monitor)
        {
            string path = ShapefilePath.Value<string>(SettingManager);
            foreach (var feature in Shapefile.ReadAllFeatures(path))
            {
                foreach (var attrName in feature.Attributes.GetNames())
                {
                    monitor.Writer.WriteLine($"{attrName,10}: {feature.Attributes[attrName]}");
                }
                monitor.Writer.WriteLine($"     SHAPE: {feature.Geometry}");
                monitor.Writer.WriteLine();
            }
        }

        public void SetPluginState(PluginState state, ISet<string>? visited = null)
        {
            this.DeployPluginState(state, PluginManager, SettingManager, visited);
        }

        public IHtValueSource GetHtValueSource(PluginState state)
        {
            return new Source(state.SettingValues[ShapefilePath.FullKey()], state.SettingValues[IDAttribute.FullKey()]);
        }

        class Source : IHtValueSource {
            string _path;
            string _idAttribute;
            JsonSerializerSettings _settings;
            static readonly IHtValue _featureType = new JsonHtValue("Feature");
            internal Source(string path, string idAttribute) {
                _path = path;
                _idAttribute =idAttribute;
                _settings = new JsonFactory().GetSettings();
            }

            public T? Cast<T>(bool fromAggregated = false) where T : class
            {
                return this as T;
            }

            public IEnumerable<IHtValue> GetRows(IWorkMonitor monitor)
            {
                foreach (var feature in Shapefile.ReadAllFeatures(_path))
                {
                    string? id = null;
                    JsonHtValue attributes = new JsonHtValue();
                    foreach (var attrName in feature.Attributes.GetNames())
                    {
                        attributes[attrName] = new JsonHtValue(feature.Attributes[attrName].ToString());
                        if (_idAttribute == attrName) {
                            id = feature.Attributes[attrName].ToString();
                        }
                    }
                    var geometry = WellKnownCoordinates.GetCoordinatesFromWellKnownText(feature.Geometry.AsText());
                    var result = new JsonHtValue();
                    result["type"] = _featureType;
                    if (id != null) {
                        result["id"] = new JsonHtValue(id);
                    }
                    result["geometry"] = JsonConvert.DeserializeObject<JsonHtValue>(JsonConvert.SerializeObject(geometry, _settings), _settings);
                    if (attributes.Count > 0) {
                        result["attributes"]=attributes;
                    }
                    yield return result;
                }
            }
        }
    }
}
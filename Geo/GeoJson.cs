using System.Collections.Generic;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Interface.Geography;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.Geography
{
    public class GeoJsonBase
    {
        public string type { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<double>? bbox { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public GeoJsonCrs? crs { get; set; }
    }

    public class GeoJsonCrs
    {
        public string type { get; set; }
        public IHtValue? properties { get; set; }
    }

    public class GeoJsonGeometry : GeoJsonBase
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IHtValue? coordinates { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<GeoJsonGeometry>? geometries { get; set; } 
    }

    public class GeoJsonFeature : GeoJsonBase, IGeoPoint
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IHtValue? properties { get; set; }
        public GeoJsonGeometry? geometry { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? MaxMetersPerPixel { get; set; }

        [JsonIgnore]
        public double Latitude
        {
            get { return geometry?.coordinates==null ? 0.0 : geometry.coordinates[1].AsDouble; }
        }

        [JsonIgnore]
        public double Longitude
        {
            get { return geometry?.coordinates==null ? 0.0 : geometry.coordinates[0].AsDouble; }
        }
    }

    // Can hold any GeoJson object: any type of geometry, feature, or feature collection
    public class GeoJson : GeoJsonBase
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IHtValue? coordinates { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<GeoJsonGeometry>? geometries { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? MaxMetersPerPixel { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IHtValue? properties { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public GeoJsonGeometry? geometry { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<GeoJsonFeature>? features { get; set; } 
    }
}

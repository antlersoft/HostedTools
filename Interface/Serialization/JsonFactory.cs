using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Newtonsoft.Json;

using com.antlersoft.HostedTools.Serialization.Internal;

namespace com.antlersoft.HostedTools.Serialization
{
    [Export(typeof(IJsonFactory))]
    public class JsonFactory : IJsonFactory
    {
        private static JsonConverter _valueConverter = new HtValueConverter();

        private static JsonSerializerSettings _indentedSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter> {_valueConverter}
        };
        private static JsonSerializerSettings _unindentedSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.None,
            Converters = new List<JsonConverter> { _valueConverter }
        };
        public JsonSerializer GetSerializer(bool useIndent = false)
        {
            return JsonSerializer.Create(useIndent ? _indentedSettings : _unindentedSettings);
        }

        public JsonSerializerSettings GetSettings(bool useIndent = false)
        {
            return useIndent ? _indentedSettings : _unindentedSettings;
        }
    }
}


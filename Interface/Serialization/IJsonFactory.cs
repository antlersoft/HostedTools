using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.Serialization
{
	/// <summary>
	/// Creates Newtonsoft.Json objects for serializing objects containing IHtValue
	/// </summary>
    public interface IJsonFactory
    {
        JsonSerializer GetSerializer(bool useIndent = false);
        JsonSerializerSettings GetSettings(bool useIndent = false);
    }
}


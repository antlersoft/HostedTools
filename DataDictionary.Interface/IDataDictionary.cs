using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.DataDictionary.Interface
{
    /// <summary>
    /// Collection of IValueDefinition objects, keyed by name
    /// </summary>
    public interface IDataDictionary : IHostedObject
    {
      IValueDefinition this[string name] { get; }
    }
}

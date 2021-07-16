using com.antlersoft.HostedTools.Interface;
using System.Collections.Generic;

namespace com.antlersoft.DataDictionary.Interface
{
  public interface IValueView : IHtValue
  {
    string Prompt { get; }
    string Description { get; }
    string EditorKey { get; }
    IValueDefinition UnderlyingDefinition { get; }
    IList<IValueViewEntry> Entries { get; }
  }
}

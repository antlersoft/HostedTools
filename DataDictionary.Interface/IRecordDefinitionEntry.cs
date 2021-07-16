using com.antlersoft.HostedTools.Framework.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.DataDictionary.Interface
{
  /// <summary>
  /// Equivalent of a column-definition in a database that allows table-valued columns
  /// </summary>
  public interface IRecordDefinitionEntry : IHostedObject
  {
    /// <summary>
    /// Must be unique within each record definitionl
    /// </summary>
    string Name { get; }
    IValueDefinition Definition { get; }
    string Prompt { get; }
    string Description { get; }
    IValueDefinition Container { get; }
  }
}

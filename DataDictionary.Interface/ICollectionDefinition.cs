using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.DataDictionary.Interface
{
  public interface ICollectionDefinition : IValueDefinition
  {
    IValueDefinition CollectedType { get; }
    int MinimumCount { get; }
    int MaximumCount { get; }
    bool CanCreateNew { get; }
    bool ByReference { get; }
    /// <summary>
    /// Interface for selecting one of an existing set of objects to add to this collection.  May be null,
    /// in which case there is no provision for selecting from an existing set of objects.
    /// </summary>
    IObjectSelector Selector(IValueDefinition typeToSelectFrom);
    /// <summary>
    /// Interface for selecting which of several sub-types of CollectedType to add a reference for or instance of
    /// </summary>
    IDefinitionSelector TypeSelector { get; }
  }
}

using com.antlersoft.HostedTools.Framework.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.DataDictionary.Interface
{
    /// <summary>
    /// Equivalent of a table in a database that allows table-valued columns
    /// </summary>
    public interface IRecordDefinition : IValueDefinition
    {
      IEnumerable<IRecordDefinitionEntry> Entries { get; }
    }
}

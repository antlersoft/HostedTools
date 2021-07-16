using com.antlersoft.HostedTools.Framework.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.DataDictionary.Interface
{
    public interface IDefinitionSelector : IHostedObject
    {
      IEnumerable<IValueDefinition> AvailableDefinitions { get; }
    }
}

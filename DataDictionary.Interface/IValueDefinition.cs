using com.antlersoft.HostedTools.Framework.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.DataDictionary.Interface
{
  public interface IValueDefinition : IHostedObject
  {
    /// <summary>
    /// Internal identifier, unique within an IDataDictionary
    /// </summary>
    string Id { get; }
    string Description { get; }
    IValueInstance GetDefaultInstance();
  }
}

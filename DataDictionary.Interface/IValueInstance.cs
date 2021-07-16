using com.antlersoft.HostedTools.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.DataDictionary.Interface
{
  public interface IValueInstance
  {
    IValueDefinition Definition { get; }
    IHtValue Value { get; }
  }
}

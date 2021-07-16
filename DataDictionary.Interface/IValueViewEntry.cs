using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.DataDictionary.Interface
{
  public interface IValueViewEntry : IValueView
  {
    IRecordDefinitionEntry Entry { get; }
  }
}

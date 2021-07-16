using com.antlersoft.HostedTools.Interface;
using System.Collections.Generic;

namespace com.antlersoft.DataDictionary.Interface
{
  public interface IPagedView
  {
    IEnumerable<IHtValue> RetrievePage(int n);
    int EstimatedPages { get; }
  }
}

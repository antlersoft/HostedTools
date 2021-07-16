using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.DataDictionary.Interface
{
    public interface IObjectSelector : IHostedObject
    {
    IEnumerable<string> ColumnNames { get; }
    IPagedView PagedView(IHtValue parameters);
    }
}

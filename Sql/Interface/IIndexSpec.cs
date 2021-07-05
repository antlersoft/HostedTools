using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface IIndexSpec
    {
        IList<IIndexColumn> Columns { get; }
    }
}

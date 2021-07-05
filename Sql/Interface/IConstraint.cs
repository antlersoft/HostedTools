using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface IConstraint
    {
        string Name { get; }
        ITable ReferencedTable { get; }
        IIndexSpec LocalColumns { get; }
        IIndexSpec ReferencedColumns { get; }
    }
}

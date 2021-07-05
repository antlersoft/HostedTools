using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface ITable : IBasicTable
    {
        IList<IConstraint> Constraints { get; }
        IIndexSpec PrimaryKey { get; }
    }
}

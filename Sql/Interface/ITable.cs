using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface ITable : IBasicTable
    {
        IList<IConstraint> Constraints { get; }
        IIndexSpec PrimaryKey { get; }

        /// <summary>
        /// Required to handle inserts of rows without deferring constraints when circular constraints
        /// </summary>
        IList<IField> ForceNullOnInsert { get; }
    }
}

using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    /// <summary>
    /// Obtains an enumeration of the referential constraints on a table identified by an IBasicTable defined in the
    /// DBMS associated with this object.  Uses a Functor to obtain the IBasicTable associated with the
    /// referenced tables's schema and name
    /// </summary>
    public interface ISqlReferentialConstraintInfo
    {
        /// <summary>
        /// Obtains an enumeration of the referential constraints on a table identified by an IBasicTable defined in the
        /// DBMS associated with this object.  Uses a Functor to obtain the IBasicTable associated with the
        /// referenced tables's schema and name
        /// </summary>
        /// <param name="table">Identified by schema and name the table in the DBMS for which to obtain info</param>
        /// <param name="tableGetter">Get an IBasicTable (for column info) for the table referenced
        /// by the constraint; arguments passed are table_schema and table_name</param>
        /// <returns>Referential constraints associated with table</returns>
        IEnumerable<IConstraint> GetReferentialConstraints(IBasicTable table, Func<string,string,ITable> tableGetter);
    }
}

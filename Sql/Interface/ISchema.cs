using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface ISchema : IHostedObject
    {
        IList<ITable> Tables { get; }
        ITable GetTable(string schemaName, string tableName);
    }
}

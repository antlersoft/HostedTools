using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface IDistinctHandling
    {
        string GetDistinctText(IEnumerable<Tuple<string, ITable>> aliasesAndTables);
    }
}

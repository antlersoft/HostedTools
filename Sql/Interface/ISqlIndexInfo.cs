using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface ISqlIndexInfo : ISqlConnectionSource
    {
        Dictionary<string,IIndexSpec> GetIndexInfo(IBasicTable table);
    }
}

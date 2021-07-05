using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface IField
    {
        string Name { get; }
        string DataType { get; }
        int OrdinalPosition { get; }
        bool Nullable { get; }
    }
}

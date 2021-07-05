using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface IIndexColumn
    {
        IField Field { get; }
        bool Descending { get; }
    }
}

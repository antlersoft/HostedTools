using com.antlersoft.HostedTools.Framework.Interface;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface IBasicTable : IHostedObject
    {
        string Name { get; }
        string Schema { get; }
        IList<IField> Fields { get; }
        IField this[string i] { get; }
    }
}

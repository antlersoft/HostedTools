using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Sql.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Archive.Interface
{
    public interface IArchive : IHostedObject
    {
        IArchiveSpec Spec { get; }
        IEnumerable<IHtValue> GetRows(ITable table);

        IEnumerable<ITable> Tables { get; }
    }
}

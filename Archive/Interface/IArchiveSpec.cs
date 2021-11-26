using com.antlersoft.HostedTools.Framework.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Archive.Interface
{
    public interface IArchiveSpec : IHostedObject
    {
        string Title { get; }
        IList<IArchiveTableSpec> TableSpecs { get; }

        bool UseCompression { get; }
    }
}

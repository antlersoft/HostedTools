using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Sql.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Archive.Interface
{
    public interface IArchiveRepository : IHostedObject
    {
        ISchema Schema { get; }
        IArchive GetArchive(IArchiveSpec spec, IWorkMonitor monitor);
        void WriteArchive(IArchive archive, IWorkMonitor monitor);
    }
}

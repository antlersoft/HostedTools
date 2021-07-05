using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.antlersoft.HostedTools.Archive.Model
{
    public class ArchiveSpec : HostedObjectBase, IArchiveSpec
    {
        public ArchiveSpec(IEnumerable<IArchiveTableSpec> tableSpecs, string title = null)
        {
            TableSpecs = tableSpecs.ToList();
            Title = title ?? Guid.NewGuid().ToString();
        }
        public IList<IArchiveTableSpec> TableSpecs { get; }

        public string Title { get; }
    }
}

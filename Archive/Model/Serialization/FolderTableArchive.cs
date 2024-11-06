using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Pipeline;
using com.antlersoft.HostedTools.Serialization;
using com.antlersoft.HostedTools.Sql.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.IO;

namespace com.antlersoft.HostedTools.Archive.Model.Serialization
{
    class FolderTableArchive
    {
        internal ITable Table;
        internal string Path;

        internal IEnumerable<IHtValue> ReadSerializedRows(IJsonFactory factory)
        {
            if (File.Exists(Path))
            {
                var sr = new FileStream(Path, FileMode.Open);
                // FromJsonStream responsible for disposing of stream
                return PipelinePlugin.FromJsonStream(sr, factory, false, false).GetRows(new ConsoleMonitor());
            }
            if (File.Exists(Path + FolderRepository.GzipSuffix))
            {
                var sr = new FileStream(Path + FolderRepository.GzipSuffix, FileMode.Open);
                return PipelinePlugin.FromJsonStream(sr, factory, true, false).GetRows(new ConsoleMonitor());
            }
            throw new Exception($"Archive file {Path} for table {Table.ToString()} not found");
        }

        class ConsoleMonitor : HostedObjectBase, IWorkMonitor
        {
            public TextWriter Writer => Console.Out;

            public Exception Thrown { get; set; }
        }
    }
}

using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Pipeline;
using com.antlersoft.HostedTools.Serialization;
using com.antlersoft.HostedTools.Sql.Interface;
using System.Collections.Generic;
using System.IO;

namespace com.antlersoft.HostedTools.Archive.Model.Serialization
{
    class FolderTableArchive
    {
        internal ITable Table;
        internal string Path;

        internal IEnumerable<IHtValue> ReadSerializedRows(IJsonFactory factory)
        {
            var sr = new FileStream(Path, FileMode.Open);
            // FromJsonStream responsible for disposing of stream
            return PipelinePlugin.FromJsonStream(sr, factory, false, false);
        }
    }
}

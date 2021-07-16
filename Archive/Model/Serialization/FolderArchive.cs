using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using com.antlersoft.HostedTools.Sql.Interface;
using System.Collections.Generic;
using System.Linq;

namespace com.antlersoft.HostedTools.Archive.Model.Serialization
{
    class FolderArchive : HostedObjectBase, IArchive
    {
        private IList<FolderTableArchive> _tables;
        private IJsonFactory Serializer { get; }

        internal FolderArchive(IJsonFactory serializer, FolderRepository repository, IArchiveSpec spec, IList<FolderTableArchive> tables)
        {
            Serializer = serializer;
            Spec = spec;
            _tables = tables;
        }
        public IArchiveSpec Spec { get; }

        public IEnumerable<ITable> Tables => _tables.Select(t => t.Table);

        public IEnumerable<IHtValue> GetRows(ITable table)
        {
            var archiveTable = _tables.First(t => t.Table.Schema == table.Schema && t.Table.Name == table.Name);
            return archiveTable.ReadSerializedRows(Serializer);
        }
    }
}

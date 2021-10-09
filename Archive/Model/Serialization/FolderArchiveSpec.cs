using com.antlersoft.HostedTools.Archive.Model.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace com.antlersoft.HostedTools.Archive.Model.Serialization
{
    public class FolderArchiveSpec
    {
        public string Title { get; set; }

        public int Version { get; set; }
        public List<FolderTableSpec> Tables { get; set; }
        public List<TableReference> TablesInArchive { get; set; }

        public bool UseCompression { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || ! (obj is FolderArchiveSpec))
            {
                return false;
            }
            var other = obj as FolderArchiveSpec;
            if (Title != other.Title)
            {
                return false;
            }

            if (Tables != null && (other.Tables == null || Tables.Count != other.Tables.Count ))
            {
                return false;
            }

            if (Tables == null && other.Tables == null)
            {
                return true;
            }

            if (UseCompression != other.UseCompression)
            {
                return false;
            }

            var ordered = Tables.OrderBy(a => a);
            var otherOrdered = other.Tables.OrderBy(a => a).ToList();

            int i = 0;
            foreach (var f in ordered)
            {
                var otherF = otherOrdered[i++];
                if (otherF.CompareTo(f) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int result = 0;
            if (Title != null)
            {
                result += Title.GetHashCode();
            }
            if (Tables != null)
            {
                foreach (var f in Tables.OrderBy(a => a))
                {
                    result |= f.GetHashCode();
                }
            }
            return result;
        }
    }
}

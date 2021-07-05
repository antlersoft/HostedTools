using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Archive.Model.Serialization
{
    public class FolderTableSpec : IComparable<FolderTableSpec>
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string FilterExpression { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is FolderTableSpec fts)
            {
                return CompareTo(fts) == 0;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (SchemaName?.GetHashCode() ?? 0) | (TableName?.GetHashCode() ?? 0) | (FilterExpression?.GetHashCode() ?? 0);
        }

        public int CompareTo(FolderTableSpec other)
        {
            int diff = SchemaName?.CompareTo(other.SchemaName) ?? (other.SchemaName == null ? 0 : -1);
            if (diff == 0)
            {
                diff = TableName.CompareTo(other.TableName);
            }
            if (diff == 0)
            {
                diff = FilterExpression?.CompareTo(other.FilterExpression) ?? (other.FilterExpression == null ? 0 : -1);
            }
            return diff;
        }
    }
}

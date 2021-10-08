using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Sql.Interface;
using System.Collections.Generic;
using System.Linq;

namespace com.antlersoft.HostedTools.Archive.Model.Configuration
{
    public class ConstraintConfiguration
    {
        public string Name;
        public TableReference ReferencedTable;
        public List<string> LocalColumns;
        public List<string> ReferencedColumns;
        public string SpecialLocalColumnValueGetter;

        public bool IsSameConstraint(ConstraintConfiguration other)
        {
            if (other == null)
            {
                return false;
            }
            if (ReferencedTable.Equals(other.ReferencedTable) && (LocalColumns?.Count??-1) == (ReferencedColumns?.Count??-2)
                && (other.LocalColumns?.Count??-3)==(other.ReferencedColumns?.Count??-4)
                && (other.LocalColumns?.Count??-5)==(LocalColumns?.Count??-6)
                && SpecialLocalColumnValueGetter == other.SpecialLocalColumnValueGetter)
            {
                for (int i = LocalColumns.Count-1; i>=0; i--)
                {
                    if (other.ReferencedColumns[i]!= ReferencedColumns[i] ||
                        other.LocalColumns[i] != LocalColumns[i])
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }

        public ConstraintConfiguration()
        {

        }

        public ConstraintConfiguration(IConstraint ic)
        {
            Name = ic.Name;
            ReferencedTable = new TableReference() { Schema = ic.ReferencedTable.Schema, Name = ic.ReferencedTable.Name };
            LocalColumns = ic.LocalColumns?.Columns?.Select(col => col.Field.Name)?.ToList();
            ReferencedColumns = ic.ReferencedColumns?.Columns?.Select(col => col.Field.Name)?.ToList();
            if (ic.Cast<ISpecialColumnValueGetter>() is ISpecialColumnValueGetter svg)
            {
                SpecialLocalColumnValueGetter = svg.Name;
            }
        }
    }
}

using com.antlersoft.HostedTools.Sql.Interface;

namespace com.antlersoft.HostedTools.Archive.Model
{
    class DependentTable
    {
        internal IConstraint Constraint;
        internal SqlArchiveTable ArchiveTable;
        internal bool ReverseDependency;

        public override string ToString()
        {
            return $"{Constraint.ReferencedTable.Schema}.{Constraint.ReferencedTable.Name} depends through {Constraint.ReferencedColumns.Columns[0].Field.Name}";
        }
    }
}

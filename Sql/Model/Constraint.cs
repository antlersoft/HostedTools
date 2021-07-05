using com.antlersoft.HostedTools.Sql.Interface;
using com.antlersoft.HostedTools.Framework.Model;

namespace com.antlersoft.HostedTools.Sql.Model
{
    public class Constraint : HostedObjectBase, IConstraint
    {
        public Constraint(string name, ITable table, IIndexSpec localColumns, IIndexSpec referencedColumns)
        {
            Name = name;
            _table = table;
            _localColumns = localColumns;
            _referencedColumns = referencedColumns;
        }
        ITable _table;
        IIndexSpec _localColumns;
        IIndexSpec _referencedColumns;

        public string Name { get; }

        public ITable ReferencedTable => _table;
        public IIndexSpec LocalColumns => _localColumns;

        public IIndexSpec ReferencedColumns => _referencedColumns;
    }
}

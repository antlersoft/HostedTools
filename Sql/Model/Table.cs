using com.antlersoft.HostedTools.Sql.Interface;
using com.antlersoft.HostedTools.Framework.Model;
using System.Collections.Generic;
using System.Linq;

namespace com.antlersoft.HostedTools.Sql.Model
{
    public class Table : HostedObjectBase, ITable
    {
        Dictionary<string, IField> _fields = new Dictionary<string, IField>();
        List<IConstraint> _constraints = new List<IConstraint>();
        IIndexSpec _primaryKey;
        List<IField> _forceNullOnInsert = new List<IField>();

        public Table(string schemaName, string tableName, bool requiredColumnListInSelect=false)
        {
            Name = tableName;
            Schema = schemaName;
            RequiredColumnListInSelect = requiredColumnListInSelect;
        }
        public void AddField(IField f)
        {
            _fields[f.Name] = f;
        }

        public void AddConstraint(IConstraint ic)
        {
            _constraints.Add(ic);
        }

        public void SetPrimaryKey(IIndexSpec key)
        {
            _primaryKey = key;
        }

        public bool RequiredColumnListInSelect { get; }

        public void SetForceNullOnInsertFields(IEnumerable<string> fieldNames)
        {
            _forceNullOnInsert.Clear();
            _forceNullOnInsert.AddRange(fieldNames.Select(n => _fields[n]));
        }

        public IList<IField> ForceNullOnInsert => _forceNullOnInsert;

        public IField this[string i] => _fields[i];

        public string Name { get; private set; }

        public string Schema { get; private set; }

        public IList<IField> Fields => _fields.Values.ToList();

        public IList<IConstraint> Constraints => _constraints;

        public IIndexSpec PrimaryKey => _primaryKey;

        public override string ToString()
        {
            return $"Table {Schema}.{Name}";
        }
    }
}

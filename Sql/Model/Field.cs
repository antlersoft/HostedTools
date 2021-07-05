using com.antlersoft.HostedTools.Sql.Interface;
using com.antlersoft.HostedTools.Framework.Model;

namespace com.antlersoft.HostedTools.Sql.Model
{
    public class Field : HostedObjectBase, IField
    {
        public Field(string field, string type, int ordinalPosition, bool nullable)
        {
            Name = field;
            DataType = type;
            OrdinalPosition = ordinalPosition;
            Nullable = nullable;
        }
        public string Name {
            get; private set;
        }

        public string DataType { get; }

        public int OrdinalPosition { get; }

        public bool Nullable { get; private set; }
    }
}

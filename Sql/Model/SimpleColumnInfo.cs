using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Sql.Interface;
using System.Text;

namespace com.antlersoft.HostedTools.Sql.Model
{
    public class SimpleColumnInfo : HostedObjectBase, ISqlColumnInfo
    {
        public StringBuilder AppendColumnLiteral(StringBuilder builder, IField field, IHtValue value)
        {
            if (builder == null)
            {
                builder = new StringBuilder();
            }
            if (value.IsLong && field.DataType.Contains("unsigned"))
            {
                builder.Append(((ulong)value.AsLong).ToString());
            }
            return SqlUtil.AppendSqlLiteral(builder, value);
        }

        public string GetColumnReference(IField field)
        {
            return field.Name;
        }
    }
}

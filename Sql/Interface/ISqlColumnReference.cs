using com.antlersoft.HostedTools.Interface;
using System.Text;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface ISqlColumnInfo
    {
        string GetColumnReference(IField field);
        StringBuilder AppendColumnLiteral(StringBuilder builder, IField field, IHtValue value);
    }
}

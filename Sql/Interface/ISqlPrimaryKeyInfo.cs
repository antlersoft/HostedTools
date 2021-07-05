
namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface ISqlPrimaryKeyInfo : ISqlConnectionSource
    {
        IIndexSpec GetPrimaryKey(IBasicTable table);
    }
}

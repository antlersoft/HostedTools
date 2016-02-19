
namespace com.antlersoft.HostedTools.Interface
{
    public interface INoSqlFactory
    {
        INoSql CreateProvider(IHtValue credentials);
    }
}

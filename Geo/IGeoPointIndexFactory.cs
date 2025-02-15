
namespace com.antlersoft.HostedTools.Interface.Geography
{
    public interface IGeoPointIndexFactory
    {
        IGeoPointIndex<T> CreatePointIndex<T>() where T : IGeoPoint;
    }
}

using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Interface.Geography
{
    public interface IGeoBoundingBoxIndexFactory
    {
        IGeoBoundingBoxIndex<T> CreateBoundingBoxIndex<T>() where T : IGeoBoundingBox;
    }
}

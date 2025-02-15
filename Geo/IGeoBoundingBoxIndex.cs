using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Interface.Geography
{
    public interface IGeoBoundingBoxIndex<T> where T : IGeoBoundingBox
    {
        void Clear();
        void Add(T bb);
        void AddRange(IEnumerable<T> bb);
        void Remove(T bb);
        IEnumerable<T> Intersecting(IGeoBoundingBox boundingBox);
        IEnumerable<T> Intersecting(IGeoPoint point);
    }
}

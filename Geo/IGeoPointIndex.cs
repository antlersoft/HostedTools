using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Interface.Geography
{
    public interface IGeoPointIndex<T> where T : IGeoPoint
    {
        void Clear();
        void Add(T point);
        void AddRange(IEnumerable<T> points);
        void Remove(T point);
        IEnumerable<T> PointsWithinBoundingBox(IGeoBoundingBox boundingBox);
        T ClosestPoint(IGeoPoint point);
        IEnumerable<T> PointsWithinRangeInKilometers(IGeoPoint point, double rangeInKilometers);
        double DistanceBetween(IGeoPoint a, IGeoPoint b);
        IEnumerable<T> ClosestNPoints(IGeoPoint point, int n);
    }
}

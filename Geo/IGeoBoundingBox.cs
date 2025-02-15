
namespace com.antlersoft.HostedTools.Interface.Geography
{
    public interface IGeoBoundingBox
    {
        double North { get; }
        double South { get; }
        double East { get; }
        double West { get; }
    }

    public static class GeoBoundingBoxExtensions
    {
        public static bool ContainsPoint(this IGeoBoundingBox box, IGeoPoint point)
        {
            if (box.East < box.West)
            {
                return ! (box.East <= point.Longitude && box.West >= point.Longitude) && box.North >= point.Latitude &&
                       box.South <= point.Latitude;
            }
            return box.East >= point.Longitude && box.West <= point.Longitude && box.North >= point.Latitude &&
                    box.South <= point.Latitude;
        }
    }
}

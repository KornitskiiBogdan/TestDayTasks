using MapLibrary.DAL;

namespace MapLibrary.Utils
{
    public interface IСoordinateConverter
    {
        GeoCoordinate ToGeoCoords(Сoordinate coordinate);
    }
}

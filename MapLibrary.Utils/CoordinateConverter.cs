using MapLibrary.DAL;
using System;

namespace MapLibrary.Utils
{
    
    public class CoordinateConverter : IСoordinateConverter
    {
        private const double EarthRadius = 6378137.0;
        private const double MaxLatitude = 85.05112878;
        private const double MinLatitude = -85.05112878;
        
        public GeoCoordinate ToGeoCoords(Сoordinate coordinate)
        {
            if (coordinate == null)
                throw new ArgumentNullException(nameof(coordinate));
               
            double lonRad = coordinate.X / EarthRadius;
            double latRad = Math.Atan(Math.Sinh(coordinate.Y / EarthRadius));
            
            double longitude = lonRad * 180.0 / Math.PI;
            double latitude = latRad * 180.0 / Math.PI;
            
            latitude = Math.Max(MinLatitude, Math.Min(MaxLatitude, latitude));
            
            return new GeoCoordinate
            {
                Longitude = longitude,
                Latitude = latitude
            };
        }
    }
}

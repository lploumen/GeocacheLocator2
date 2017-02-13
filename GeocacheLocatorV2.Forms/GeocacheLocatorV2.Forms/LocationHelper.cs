using System;
using Plugin.Geolocator.Abstractions;

namespace GeocacheLocatorV2.PCL
{
    public class LocationHelper
    {
        public static double CacheAzimuth(Position currentCoordinate, Position soughtCoordinate)
        {
            double y = Math.Sin((soughtCoordinate.Longitude - currentCoordinate.Longitude) * Math.PI / 180) * Math.Cos(soughtCoordinate.Latitude * Math.PI / 180);
            double x = Math.Cos(currentCoordinate.Latitude * Math.PI / 180) * Math.Sin(soughtCoordinate.Latitude * Math.PI / 180) -
                       Math.Sin(currentCoordinate.Latitude * Math.PI / 180) * Math.Cos(soughtCoordinate.Latitude * Math.PI / 180) * Math.Cos((soughtCoordinate.Longitude - currentCoordinate.Longitude) * Math.PI / 180);
            double cacheAzimuth = (Math.Atan2(y, x) * 180 / Math.PI + 360) % 360;

            return cacheAzimuth;
        }

        public static float Distance(Position point,Position other)
        {
            const double rad = Math.PI / 180;

            // Use the Harversine Formula (Great circle), have a look to:
            // http://en.wikipedia.org/wiki/Great-circle_distance
            // http://williams.best.vwh.net/avform.htm (Aviation Formulary)
            double lon1 = rad * -point.Longitude;
            double lat1 = rad * point.Latitude;
            double lon2 = rad * -other.Longitude;
            double lat2 = rad * other.Latitude;

            double d = 2 * Math.Asin(Math.Sqrt(
                Math.Pow(Math.Sin((lat1 - lat2) / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin((lon1 - lon2) / 2), 2)
            ));
            return (float)(1852 * 60 * d / rad);
        }

    }
}
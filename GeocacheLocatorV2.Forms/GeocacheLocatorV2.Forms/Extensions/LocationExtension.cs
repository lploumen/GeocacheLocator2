using GeocachingToolbox;
using Plugin.Geolocator.Abstractions;

namespace GeocacheLocatorV2.PCL.Extensions
{
    public static class LocationExtension
    {
        public static Position ToPosition(this Location loc)
        {
            return new Position() {Latitude = (double) loc.Latitude, Longitude = (double) loc.Longitude};
        }
    }
}
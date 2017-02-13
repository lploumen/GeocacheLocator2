namespace GeocacheLocatorV2.PCL.Converters
{
    class DistanceConverter
    {
        public enum Unit
        {
            Metric,
            Imperial
        }

        public static string DistanceToText(double distanceInMeters, Unit destinationUnit, int decimalDigits = 2)
        {
            string format = "0";
            if (decimalDigits > 0)
                format = "N" + decimalDigits;

            if (distanceInMeters >= 1000)
                return $"{(distanceInMeters / 1000.0).ToString(format)} Km";
            return $"{distanceInMeters.ToString(format)} m";
        }
    }
}

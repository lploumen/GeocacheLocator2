namespace GeocacheLocatorV2.PCL
{
    public class CompassHelper
    {

        public static double CalculateNormalDifference(double lastDirection, double currentDirection)
        {
            double difference = currentDirection - lastDirection;
            return NormalizeAngle(difference);
        }

        public static double NormalizeAngle(double angle)
        {
            angle %= 360;
            if (angle < -180)
            {
                angle += 360;
            }
            if (angle > 180)
            {
                angle -= 360;
            }
            return angle;
        }
    }
}
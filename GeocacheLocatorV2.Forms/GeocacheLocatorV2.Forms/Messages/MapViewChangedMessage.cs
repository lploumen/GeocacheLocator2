using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace GeocacheLocatorV2.PCL.Messages
{
    public class MapViewChangedMessage
    {
        public Rectangle Bounds { get; internal set; }
        public Position Center { get; internal set; }

        public MapViewChangedMessage(Rectangle bounds,Position center)
        {
            Bounds = bounds;
            Center = center;
        }
    }
}

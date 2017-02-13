using Xamarin.Forms.Maps;

namespace GeocacheLocatorV2.PCL.Messages
{
    public class MapChangeViewMessage
    {
        public MapSpan MapSpan { get; internal set; }

        public MapChangeViewMessage(MapSpan mapSpan)
        {
            MapSpan = mapSpan;
        }
    }
}
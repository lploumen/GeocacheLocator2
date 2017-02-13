using GeocacheLocatorV2.PCL.ViewModels;

namespace GeocacheLocatorV2.PCL.Messages
{
    public class GeocacheDownloadedMessage
    {
        public ILocationViewModel PinVM { get; }

        public GeocacheDownloadedMessage(ILocationViewModel pinVM)
        {
            PinVM = pinVM;
        }
    }
}
using System.Windows.Input;
using GeocachingToolbox;

namespace GeocacheLocatorV2.PCL.ViewModels
{
    public class GeocachePinViewModel : ILocationViewModel
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool Found { get; set; }
        public bool Own { get; set; }
        public ICommand Command { get; set; }
        public GeocacheType CacheType { get; set; }
        public GeocacheStatus CacheStatus { get; set; }
        public bool IsDetailed { get; set; }
    }
}
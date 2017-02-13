using System;
using System.Collections.Generic;
using System.Windows.Input;
using GeocachingToolbox;

namespace GeocacheLocatorV2.PCL.ViewModels
{
    public interface ILocationViewModel
    {
        string Code { get; set; }
        string Description { get; }
        double Latitude { get; }
        double Longitude { get; }
        bool Found { get; set; }
        bool IsDetailed { get; set; }
        bool Own { get; set; }
        ICommand Command { get; }
        GeocacheType CacheType { get; }
        GeocacheStatus CacheStatus { get; }
    }

    public class LocationViewModelComparer : IEqualityComparer<ILocationViewModel>
    {
        public bool Equals(ILocationViewModel x, ILocationViewModel y)
        {
            if (object.ReferenceEquals(x, y)) return true;
            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null)) return false;
            return x.Code == y.Code;
        }

        public int GetHashCode(ILocationViewModel obj)
        {
            if (object.ReferenceEquals(obj, null)) return 0;
            return obj.Code.GetHashCode();
        }
    }
}
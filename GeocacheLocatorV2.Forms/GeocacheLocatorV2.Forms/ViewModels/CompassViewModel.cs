using Caliburn.Micro;
using GeocacheLocatorV2.PCL.Converters;
using GeocacheLocatorV2.PCL.Extensions;
using GeocacheLocatorV2.PCL.Resources;
using GeocachingToolbox;
using Position = Plugin.Geolocator.Abstractions.Position;

namespace GeocacheLocatorV2.PCL.ViewModels
{
    public class CompassViewModel : Screen, ICompassAware, ILocationAware
    {
        private double m_cacheDirection;
        private double m_cacheAzimut;
        private Geocache m_geocache;
        private string m_distanceText;
        public bool IsNeedHighAccuracy => true;

        public double CacheDirection
        {
            get { return m_cacheDirection; }
            set
            {
                if (value.Equals(m_cacheDirection)) return;
                m_cacheDirection = value;
                NotifyOfPropertyChange();
            }
        }

        protected override async void OnActivate()
        {
            base.OnActivate();
            SmoothCompassManager.Instance.AddSubscriber(this);
            await LocationManager.Instance.AddSubscriber(this);
            DistanceText = AppResource.UpdatingLocation;
        }

        protected override async void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            SmoothCompassManager.Instance.RemoveSubscriber(this);
            await LocationManager.Instance.RemoveSubscriber(this);
        }

        public Geocache Parameter
        {
            get { return m_geocache; }
            set
            {
                if (Equals(value, m_geocache)) return;
                m_geocache = value;
                NotifyOfPropertyChange();
            }
        }

        public void SetDirection(double northDirection)
        {
            CacheDirection = -northDirection + m_cacheAzimut;
        }

        public string DistanceText
        {
            get { return m_distanceText; }
            set
            {
                if (value == m_distanceText) return;
                m_distanceText = value;
                NotifyOfPropertyChange();
            }
        }
        
        public void ProcessLocation(Position location)
        {
            if (Parameter != null)
            {
                float distance = LocationHelper.Distance(Parameter.Waypoint.ToPosition(), location);

                DistanceText =
                    $"{AppResource.Distance} : {DistanceConverter.DistanceToText(distance, DistanceConverter.Unit.Imperial)} (+-{DistanceConverter.DistanceToText(location.Accuracy, DistanceConverter.Unit.Metric, 0)})";
                m_cacheAzimut = LocationHelper.CacheAzimuth(location, Parameter.Waypoint.ToPosition());
            }
        }
    }
}

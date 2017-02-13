using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace GeocacheLocatorV2.PCL
{
    public enum GeoPositionAccuracy
    {
        Default = 20,
        High = 1
    }

    public interface ILocationAware
    {
        bool IsNeedHighAccuracy { get; }
        void ProcessLocation(Position location);
    }


    public class LocationManager
    {
        private readonly List<ILocationAware> _subscribers = new List<ILocationAware>();
        private IGeolocator _watcher;
        private bool _isLocationEnabled;

        private static LocationManager _instance;
        public bool UserLocationAllowed { get; set; }

        public static LocationManager Instance => _instance ?? (_instance = new LocationManager());

        private LocationManager()
        {
            _isLocationEnabled = true;
            SetNewWatcher(GeoPositionAccuracy.Default);
        }

        private void SetNewWatcher(GeoPositionAccuracy accuracy)
        {
            var movementThreshold = accuracy == GeoPositionAccuracy.High ? 1 : 20;
            _watcher = CrossGeolocator.Current;
            _watcher.DesiredAccuracy = movementThreshold;
            _watcher.PositionChanged += PositionChanged;
        }

        public async Task UpdateIsLocationEnabled(bool newValue)
        {
            _isLocationEnabled = newValue;

            if (_isLocationEnabled)
                await StartWatcher();
            else
                await StopWatcher();
        }

        public async Task AddSubscriber(ILocationAware locationAware)
        {
            if (!UserLocationAllowed)
                return;
            if (locationAware != null)
            {
                if (!_subscribers.Contains(locationAware))
                    _subscribers.Add(locationAware);

                if (locationAware.IsNeedHighAccuracy && Math.Abs(_watcher.DesiredAccuracy - (int)GeoPositionAccuracy.Default) < 0.1)
                {
                    await StopWatcher();
                    SetNewWatcher(GeoPositionAccuracy.High);
                    await StartWatcher();
                }
            }

            if (_subscribers.Count == 1 && _isLocationEnabled)
            {
                await StartWatcher();
            }
        }

        public async Task RemoveSubscriber(ILocationAware locationAware)
        {
            if (_subscribers.Contains(locationAware))
                _subscribers.Remove(locationAware);

            if (_subscribers.Count == 0)
                await StopWatcher();

            if (locationAware.IsNeedHighAccuracy)
            {
                if (_subscribers.Any(c => c.IsNeedHighAccuracy))
                    return;
                SetNewWatcher(GeoPositionAccuracy.Default);
            }
        }

        private async Task StartWatcher()
        {
            if (!_watcher.IsListening)
                await _watcher.StartListeningAsync(300, 1);
        }

        private async Task StopWatcher()
        {
            await _watcher.StopListeningAsync();
            // _watcher.Stop();
        }

        private void PositionChanged(object sender, PositionEventArgs e)
        {
            foreach (var c in _subscribers)
            {
                c.ProcessLocation(e.Position);
            }
        }
    }
}

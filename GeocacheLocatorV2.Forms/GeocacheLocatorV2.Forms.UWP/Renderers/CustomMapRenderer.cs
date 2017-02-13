using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
using Caliburn.Micro;
using GeocacheLocatorV2.PCL.Controls;
using GeocacheLocatorV2.PCL.ViewModels;
using GeocacheLocatorV2.UWP.Controls;
using GeocacheLocatorV2.UWP.Renderers;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.UWP;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CustomMap), typeof(CustomMapRenderer))]
namespace GeocacheLocatorV2.UWP.Renderers
{
    class CustomMapRenderer : MapRenderer
    {
        private IEventAggregator _eventAggregator;
        MapControl nativeMap;
        private CustomMap _CustomMap;
        private UserLocation m_UserLocation;
        protected override void OnElementChanged(ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                nativeMap.Children.Clear();
                nativeMap.MapElements.Clear();
                _CustomMap.PinAdded -= CustomMapOnPinAdded;
                _CustomMap.PinRemoved -= CustomMapOnPinRemoved;
                _CustomMap.UserPositionChanged -= CustomMapOnUserPositionChanged;
                _CustomMap.UserOrientationChanged -= CustomMapOnUserOrientationChanged;
                _CustomMap.UpdatePin -= CustomMapOnUpdatePin;
                nativeMap = null;
            }

            if (e.NewElement != null)
            {
                _CustomMap = (CustomMap)e.NewElement;
                _CustomMap.Pins.Clear();
                nativeMap = Control;
                _CustomMap.PinAdded += CustomMapOnPinAdded;
                _CustomMap.PinRemoved += CustomMapOnPinRemoved;
                _CustomMap.UserPositionChanged += CustomMapOnUserPositionChanged;
                _CustomMap.UserOrientationChanged += CustomMapOnUserOrientationChanged;
                _CustomMap.UpdatePin += CustomMapOnUpdatePin;
                _eventAggregator = IoC.Get<IEventAggregator>();
                _eventAggregator.Subscribe(this);
            }
        }

        private void CustomMapOnUpdatePin(ILocationViewModel locationViewModel)
        {
            var children = nativeMap.Children;
            foreach (var child in children)
            {
                GeoPushPin geo = child as GeoPushPin;
                if (geo != null)
                {
                    if (geo.Cache.Code == locationViewModel.Code)
                    {
                        geo.Update(locationViewModel);
                    }
                }
            }
        }

        private async void CustomMapOnUserOrientationChanged(double orientationDeg)
        {
            CoreDispatcher dispatcher = nativeMap.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                m_UserLocation?.Rotate(
                    (float)orientationDeg);
            });
        }

        private async void CustomMapOnUserPositionChanged(object sender, PositionEventArgs pos)
        {
            CoreDispatcher dispatcher = nativeMap.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (m_UserLocation == null)
                {
                    m_UserLocation = new UserLocation();
                    nativeMap.Children.Add(m_UserLocation);
                    MapControl.SetNormalizedAnchorPoint(m_UserLocation, new Point(0.5, 0.5));
                }
                BasicGeoposition snPosition = new BasicGeoposition()
                {
                    Latitude = pos.Position.Latitude,
                    Longitude = pos.Position.Longitude
                };
                Geopoint snPoint = new Geopoint(snPosition);
                MapControl.SetLocation(m_UserLocation, snPoint);
            });
        }

        public static int RemoveAll<T>(IList<T> list, Predicate<T> match)
        {
            int count = 0;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i]))
                {
                    ++count;
                    list.RemoveAt(i);
                }
            }
            return count;
        }

        private void CustomMapOnPinRemoved(IReadOnlyList<ILocationViewModel> locationViewModels)
        {
            if (nativeMap != null)
            {
                Debug.WriteLine($"Remove {locationViewModels.Count}");
                RemoveAll(nativeMap.Children, c => (c is GeoPushPin) && locationViewModels.Contains(((GeoPushPin)c).Cache, new LocationViewModelComparer()));
            }
        }

        private void CustomMapOnPinAdded(IReadOnlyList<ILocationViewModel> locationViewModels)
        {
            foreach (var vm in locationViewModels)
                AddPin(vm);
        }

        private void AddPin(ILocationViewModel vm)
        {
            BasicGeoposition snPosition = new BasicGeoposition() { Latitude = vm.Latitude, Longitude = vm.Longitude };
            Geopoint snPoint = new Geopoint(snPosition);
            var uc = new GeoPushPin(vm);
            uc.Tapped += UcOnTapped;
            nativeMap.Children.Add(uc);
            MapControl.SetLocation(uc, snPoint);
            MapControl.SetNormalizedAnchorPoint(uc, new Point(0.5, 1.0));
        }

        private void UcOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            var pin = sender as GeoPushPin;
            if (pin != null)
                _CustomMap.PinClicked(pin.Cache);
        }
    }
}

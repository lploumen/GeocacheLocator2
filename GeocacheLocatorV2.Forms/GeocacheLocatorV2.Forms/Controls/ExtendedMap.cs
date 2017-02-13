using System;
using System.Collections.Generic;
using System.Diagnostics;
using Caliburn.Micro;
using GeocacheLocatorV2.PCL.Messages;
using GeocacheLocatorV2.PCL.Utils;
using GeocacheLocatorV2.PCL.ViewModels;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace GeocacheLocatorV2.PCL.Controls
{
    public class CustomMap : Map, ILocationAware, IHandle<GeocacheDownloadedMessage>,ICompassAware
    {
        private readonly Delayer m_DelayBounds = new Delayer(TimeSpan.FromMilliseconds(500));
        public event EventHandler<PositionEventArgs> UserPositionChanged;
        public event Action<double> UserOrientationChanged;
        public event Action<ILocationViewModel> UpdatePin;
        public event Action<IReadOnlyList<ILocationViewModel>> PinAdded;
        public event Action<IReadOnlyList<ILocationViewModel>> PinRemoved;
        public bool IsNeedHighAccuracy => false;

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty
           .Create<CustomMap, MtObservableCollection<ILocationViewModel>>(
               p => p.ItemsSource, null, BindingMode.Default, null, ItemsSourceChanged);

        private int cnt = 0;
        public CustomMap()
        {
            var ea = IoC.Get<IEventAggregator>();
            ea.Subscribe(this);

            m_DelayBounds.Action += (sender, args) =>
            {
                if (VisibleRegion != null && IsVisible)
                {
                    Debug.WriteLine("VisibleRegion action");
                    var bounds = CalculateBoundingCoordinates(VisibleRegion);
                    ea.BeginPublishOnUIThread(new MapViewChangedMessage(bounds, VisibleRegion.Center));
                }
                m_DelayBounds.Stop();
            };

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "VisibleRegion")
                {
                    if (IsVisible)
                    {
                        m_DelayBounds.ResetAndTick();
                    }
                }
            };
        }

        public void ProcessLocation(Plugin.Geolocator.Abstractions.Position location)
        {
            UserPositionChanged?.Invoke(this, new PositionEventArgs(location));
        }


        public MtObservableCollection<ILocationViewModel> ItemsSource
        {
            get { return (MtObservableCollection<ILocationViewModel>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void ItemsSourceChanged(BindableObject bindable, MtObservableCollection<ILocationViewModel> oldValue, MtObservableCollection<ILocationViewModel> newValue)
        {
            var customMap = bindable as CustomMap;
            if (newValue != null)
            {
                newValue.ExtendedCollectionChanged += customMap.NewValueOnExtendedCollectionChanged;
            }
        }

        private void NewValueOnExtendedCollectionChanged(object sender, MtNotifyCollectionChangedEventArgs<ILocationViewModel> ea)
        {
            if (ea.AddedItems != null && ea.AddedItems.Count > 0)
            {
                PinAdded?.Invoke(ea.AddedItems);
            }
            else if (ea.RemovedItems != null && ea.RemovedItems.Count > 0)
            {
                PinRemoved?.Invoke(ea.RemovedItems);
            }
        }

        public void PinClicked(ILocationViewModel viewModel)
        {
            viewModel?.Command.Execute(viewModel);
        }

        static Rectangle CalculateBoundingCoordinates(MapSpan region)
        {
            if (Device.OS == TargetPlatform.Windows)
            {
                //TODO: Remove when bug 42151 is fixed
                //Visible region is wrong on UWP
                //https://bugzilla.xamarin.com/show_bug.cgi?id=42151
                region = new MapSpan(region.Center, region.LatitudeDegrees * 2, region.LongitudeDegrees * 2);
            }

            // WARNING: I haven't tested the correctness of this exhaustively!
            var center = region.Center;
            var halfheightDegrees = region.LatitudeDegrees / 2.0; // / 1.10;/// 2;
            var halfwidthDegrees = region.LongitudeDegrees / 2.0; // 1.10;/// 2;

            var left = center.Longitude - halfwidthDegrees;
            var right = center.Longitude + halfwidthDegrees;
            var top = center.Latitude + halfheightDegrees;
            var bottom = center.Latitude - halfheightDegrees;

            // Adjust for Internation Date Line (+/- 180 degrees longitude)
            if (left < -180) left = 180 + (180 + left);
            if (right > 180) right = (right - 180) - 180;
            // I don't wrap around north or south; I don't think the map control allows this anyway
            return new Rectangle(left, top, right - left, bottom - top);
        }

        public void Handle(GeocacheDownloadedMessage message)
        {
            UpdatePin?.Invoke(message.PinVM);
        }

        public void SetDirection(double direction)
        {
            UserOrientationChanged?.Invoke(direction);
        }
    }
}

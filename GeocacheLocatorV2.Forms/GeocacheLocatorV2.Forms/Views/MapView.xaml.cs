using System;
using System.Diagnostics;
using Caliburn.Micro;
using GeocacheLocatorV2.PCL.Messages;
using Xamarin.Forms;

namespace GeocacheLocatorV2.PCL.Views
{
    public partial class MapView : ContentPage, IHandle<MapChangeViewMessage>
    {
        private int m_ZoomLevel = 13;
        public MapView()
        {
            InitializeComponent();
            IoC.Get<IEventAggregator>().Subscribe(this);
            Appearing += OnAppearing;
            Disappearing += OnDisappearing;
        }

        private async void OnDisappearing(object sender, EventArgs eventArgs)
        {
            try
            {
                SmoothCompassManager.Instance.RemoveSubscriber(MyMap);
                await LocationManager.Instance.RemoveSubscriber(MyMap);

            }
            catch (Exception ex)
            {
                await this.DisplayAlert("title", ex.Message, "e");
            }
        }

        private async void OnAppearing(object sender, EventArgs eventArgs)
        {
            await LocationManager.Instance.AddSubscriber(MyMap);
            SmoothCompassManager.Instance.AddSubscriber(MyMap);
        }

        public void Handle(MapChangeViewMessage message)
        {
            Debug.WriteLine($"{message.MapSpan.Center.Longitude} - {message.MapSpan.Center.Latitude} - {message.MapSpan.LatitudeDegrees} - {message.MapSpan.LongitudeDegrees}");

            if (MyMap.VisibleRegion == null || (MyMap.VisibleRegion.Center != message.MapSpan.Center))
                MyMap.MoveToRegion(message.MapSpan);
        }
    }
}

using System;
using Caliburn.Micro;
using Xamarin.Forms;

namespace GeocacheLocatorV2.PCL.Utils
{
    public class Delayer
    {
        private readonly IAdvancedTimer _timer;
        public Delayer(TimeSpan timeSpan)
        {
            _timer = DependencyService.Get<IAdvancedTimer>(DependencyFetchTarget.NewInstance); // new DispatcherTimer() { Interval = timeSpan };
            _timer.InitTimer((int) timeSpan.TotalMilliseconds, Timer_Tick, true);
            _timer.StartTimer();
        }

        public event RoutedEventHandler Action;

        private void Timer_Tick()
        {
            _timer.StopTimer();
            Action?.Invoke(this, new RoutedEventArgs());
        }

        public void ResetAndTick()
        {
            _timer.StopTimer();
            _timer.StartTimer();
        }

        public void Stop()
        {
            _timer.StopTimer();
        }
    }
}

using System;
using System.Collections.Generic;
using Plugin.Compass;
using Plugin.Compass.Abstractions;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;

namespace GeocacheLocatorV2.PCL
{
    public interface ICompassAware
    {
        void SetDirection(double direction);
    }

    public class SmoothCompassManager : ILocationAware
    {
        private const int LongSleep = 60;
        private const int DefaultSleep = 20;

        private const float ArrivedEps = 0.65f;
        private const float LeavedEps = 2.5f;
        private const float SpeedEps = 0.55f;

        //private readonly Compass compass;
        private readonly IAdvancedTimer timer;

        private double goalDirection;
        private double needleDirection;
        private double speed;

        private readonly List<ICompassAware> subscribers = new List<ICompassAware>();

        private bool isArrived; // The needle has not arrived the goalDirection
        private Position m_PreviousPosition;

        private static SmoothCompassManager instance;
        private readonly bool m_CompassSupported;

        public event Action<double> HeadingAccurryChanged;
        //public ICompassCalibrate CompassCalibrator { get; set; }
        public event Action Calibrate;
        public double HeadingAccuracy { get; set; }

        public static SmoothCompassManager Instance => instance ?? (instance = new SmoothCompassManager());

        public static bool CompassSimulate { get; set; }

        public void AddSubscriber(ICompassAware compassView)
        {
            if (compassView != null)
            {
                subscribers.Add(compassView);
            }

            if (subscribers.Count == 1)
            {
                Start();
            }
        }

        public void RemoveSubscriber(ICompassAware compassView)
        {
            subscribers.Remove(compassView);

            if (subscribers.Count == 0)
            {
                Stop();
            }
        }

        private SmoothCompassManager()
        {
            m_CompassSupported = CrossCompass.Current.IsSupported;
            if (CompassSimulate)
                m_CompassSupported = false;

            if (!m_CompassSupported)
            {
                //GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                //watcher.MovementThreshold = 0;
                //watcher.Start();
                //watcher.PositionChanged += watcher_PositionChanged;
            }
            else
            {
                CrossCompass.Current.CompassChanged += CompassCurrentValueChanged;
                timer = DependencyService.Get<IAdvancedTimer>(DependencyFetchTarget.NewInstance);
                timer.InitTimer(DefaultSleep, TimerTick, true);
            }
        }


        private void TimerTick()
        {
            Paint();
        }

        private void Paint()
        {
            if (IsNeedPainting())
            {
                isArrived = false;
                if (m_CompassSupported)
                {
                    var difference = CompassHelper.CalculateNormalDifference(needleDirection, goalDirection);
                    speed = CalculateSpeed(difference, speed);
                    needleDirection = needleDirection + speed;
                }
                else
                {
                    needleDirection = goalDirection;
                }

                foreach (var c in subscribers)
                {
                    c.SetDirection(needleDirection);
                }
            }
            else
            {
                isArrived = true;
            }
            if (timer != null)
                timer.Interval = isArrived ? LongSleep : DefaultSleep;//TimeSpan.FromMilliseconds(isArrived ? LongSleep : DefaultSleep);
        }


        private void CompassCurrentValueChanged(object sender, CompassChangedEventArgs e)
        {
            double newDirection = e.Heading;
            ComputeGoalDirection(newDirection);

            //if (HeadingAccuracy != eensorReading.HeadingAccuracy)
            //{
            //    HeadingAccuracy = e.SensorReading.HeadingAccuracy;
            //    HeadingAccurryChanged?.Invoke(HeadingAccuracy);
            //}
        }

        private void ComputeGoalDirection(double newDirection)
        {
            double difference = newDirection - goalDirection;
            difference = CompassHelper.NormalizeAngle(difference);

            float divider = 2;
            if (m_CompassSupported) // smooth the movement when using a real compass
                divider = 4;
            newDirection = goalDirection + difference / divider;
            newDirection = CompassHelper.NormalizeAngle(newDirection);

            goalDirection = newDirection;
        }

        private static double CalculateSpeed(double difference, double oldSpeed)
        {
            double newSpeed;
            newSpeed = oldSpeed * 0.75f;
            newSpeed += difference / 12.5f;//25.0f;

            return newSpeed;
        }

        private bool IsNeedPainting()
        {
            if (!m_CompassSupported)
                return true;
            if (isArrived)
            {
                return Math.Abs(needleDirection - goalDirection) > LeavedEps;
            }
            return Math.Abs(needleDirection - goalDirection) > ArrivedEps || Math.Abs(speed) > SpeedEps;
        }

        private void Start()
        {
            if (!m_CompassSupported)
            {
                LocationManager.Instance.AddSubscriber(this);
                return;
            }

            try
            {
                CrossCompass.Current.Start();
                timer.StartTimer();
            }
            catch (InvalidOperationException)
            {
                //TODO: show message
            }
        }

        private void Stop()
        {
            if (!m_CompassSupported)
            {
                LocationManager.Instance.RemoveSubscriber(this);
                return;
            }
            timer.StopTimer();
            //TODO: if (CrossCompass.Current != null) // ca prend bcp de temps pour l'arreter...
            // commenté temporairement.
            //    CrossCompass.Current.Stop();
        }


        public bool IsNeedHighAccuracy => true;
        public void ProcessLocation(Position location)
        {
            //Given latitudes Φ1 and Φ2, and longitudes λ1 and λ2 (you'll need to convert them to radians):
            //Θ = atan2( sin(Φ1) * Δλ, ΔΦ)
            // rad = 180*PI/180
            if (m_PreviousPosition != null && !m_PreviousPosition.Equals(location))
            {
                double Lat = location.Latitude * Math.PI / 180.0;
                double PrevLat = m_PreviousPosition.Latitude * Math.PI / 180.0;
                double Long = location.Longitude * Math.PI / 180.0;
                double PrevLong = m_PreviousPosition.Longitude * Math.PI / 180.0;
                var resRad = Math.Atan2(Math.Sin(Lat) * (Long - PrevLong), Lat - PrevLat);
                var resDeg = (resRad * 180.0 / Math.PI + 360) % 360;

                //if (Math.Abs(resDeg) > 0.1)
                ComputeGoalDirection(resDeg);
                Paint();
            }
            m_PreviousPosition = location;
        }
    }
}
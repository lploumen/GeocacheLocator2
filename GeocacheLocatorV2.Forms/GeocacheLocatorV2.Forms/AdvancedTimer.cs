using System;

namespace GeocacheLocatorV2.PCL
{
    public interface IAdvancedTimer
    {
        /// <summary>
        /// Used for initializing timer and options
        /// </summary>
        void InitTimer(int interval, Action action, bool autoReset);

        /// <summary>
        /// Used for starting timer
        /// </summary>
        void StartTimer();

        /// <summary>
        /// Used for stopping timer
        /// </summary>
        void StopTimer();

        /// <summary>
        /// Used for checking timer status
        /// </summary>
		bool IsTimerEnabled { get; }

        /// <summary>
        /// Used for checking timer interval
        /// </summary>
		int Interval { get; set; }

    }
}

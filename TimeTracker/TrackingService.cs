using System;
using TimeTracker.Model;

namespace TimeTracker
{
    /// <summary>
    /// A helper service managing tracking
    /// </summary>
    public class TrackingService
    {
        private DateTimeOffset _startTime;

        /// <summary>
        /// Stores whether we are currently tracking
        /// </summary>
        public bool Tracking { get; private set; }

        /// <summary>
        /// Returns tracking start time
        /// </summary>
        public DateTimeOffset StartTime
        {
            get
            {
                if (!Tracking)
                {
                    throw new TrackingServiceException("Cannot return tracking start time - tracking has not started.");
                }

                return _startTime;
            }

            private set { _startTime = value; }
        }

        /// <summary>
        /// Starts tracking
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset Start()
        {
            if (Tracking)
            {
                throw new TrackingServiceException("Tracking has already started.");
            }

            this.Tracking = true;
            this.StartTime = DateTimeOffset.Now;

            return this.StartTime;
        }

        /// <summary>
        /// Stops tracking
        /// </summary>
        /// <returns>The resulting TimeTrackerData</returns>
        public TimeTrackerData Stop()
        {
            if (!Tracking)
            {
                throw new TrackingServiceException("Tracking was not started.");
            }

            this.Tracking = false;

            return new TimeTrackerData(_startTime, DateTimeOffset.Now);
        }

        /// <summary>
        /// Returns the elapsed time since tracking started
        /// </summary>
        public String Elapsed
        {
            get
            {
                if (!Tracking)
                {
                    throw new TrackingServiceException("Tracking was not started.");
                }

                System.TimeSpan timeSpan = DateTimeOffset.Now.Subtract(this.StartTime);

                return timeSpan.Format();
            }
        }
    }

    /// <summary>
    /// Tracking Service Exception
    /// </summary>
    public class TrackingServiceException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public TrackingServiceException(string message) : base(message)
        {
        }
    }
}
using System;

namespace TimeTracker.Model
{
    /// <summary>
    /// Represents TimeTracker Data Entry
    /// </summary>
    public class TimeTrackerData : ITimeTrackerData
    {
        /// <summary>
        /// <see cref="ITimeTrackerData.StartTime"/>
        /// </summary>
        public DateTimeOffset StartTime { get; }

        /// <summary>
        /// <see cref="ITimeTrackerData.EndTime"/>
        /// </summary>
        public DateTimeOffset EndTime { get; }

        /// <summary>
        /// <see cref="ITimeTrackerData.Category"/>
        /// </summary>
        public TrackedDataCategory Category { get; set; }

        /// <summary>
        /// Fake field to custom-format time-span
        /// <see cref="ITimeTrackerData.TimeElapsed"/>
        /// </summary>
        public String TimeElapsed
        {
            get
            {
                return this.GetTimeElapsed().Format();
            }
        }

        /// <param name="endTime"></param>
        /// <param name="category"></param>
        public TimeTrackerData(DateTimeOffset endTime, TrackedDataCategory category = null)
        {
            StartTime = DateTimeOffset.Now;
            EndTime = endTime;
            Category = category;
        }

        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="category"></param>
        public TimeTrackerData(DateTimeOffset startTime, DateTimeOffset endTime, TrackedDataCategory category = null)
        {
            StartTime = startTime;
            EndTime = endTime;
            Category = category;
        }

        /// <summary>
        /// Returns the time span between start and end date/time
        /// <see cref="ITimeTrackerData.GetTimeElapsed"/>
        /// </summary>
        public TimeSpan GetTimeElapsed()
        {
            return EndTime.Subtract(StartTime);
        }
    }
}
using System;

namespace TimeTracker.Model
{
    /// <summary>
    /// TimeTrackerData Entity Interface
    /// </summary>
    public interface ITimeTrackerData
    {
        /// <summary>
        /// Represents time when tracker was started
        /// </summary>
        DateTimeOffset StartTime { get; }

        /// <summary>
        /// Represents time when tracker was ended
        /// </summary>
        DateTimeOffset EndTime { get; }

        /// <summary>
        /// Represents this entry's category
        /// </summary>
        TrackedDataCategory Category { get; set; }

        /// <summary>
        /// Contains time elapsed as a user-friendly string
        /// </summary>
        String TimeElapsed { get; }

        /// <summary>
        /// Represents the difference between start time and end time
        /// </summary>
        TimeSpan GetTimeElapsed();
    }
}

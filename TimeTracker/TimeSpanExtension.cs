using System;
using System.Linq;
using System.Collections.Generic;

namespace TimeTracker
{
    /// <summary>
    /// Extends <see cref="System.TimeSpan"/> with convenience methods
    /// </summary>
    public static class TimeSpanExtension
    {
        /// <summary>
        /// Sums TimeSpan values
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns>The new, summed TimeSpan</returns>
        public static TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> selector)
        {
            var ts = new TimeSpan();
            return source.Aggregate(ts, (current, entry) => current + selector(entry));
        }

        /// <summary>
        /// Formats a TimeSpan value
        /// </summary>
        /// <param name="timeSpan">The timespan to be formatted</param>
        /// <returns>The formatted string</returns>
        public static String Format(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays > 1)
                return timeSpan.ToString("d\\d\\ hh\\:mm\\:ss");

            if (timeSpan.TotalHours > 1)
                return timeSpan.ToString("h\\:mm\\:ss");

            return timeSpan.ToString("m\\:ss");
        }
    }
}

using System;
using System.ComponentModel;
using TimeTracker.Model;

namespace TimeTracker
{
    /// <summary>
    /// TimeTracker Data Serializer
    /// </summary>
    static class DataSerializer
    {
        /// <summary>
        /// Serializes a whole data set
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Serialize(BindingList<TimeTrackerData> data)
        {
            string result = "";
            foreach (TimeTrackerData value in data)
            {
                result += SerializeValue(value) + '\n';
            }

            return result;
        }

        /// <summary>
        /// Serializes a single Time Tracker Data Value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SerializeValue(ITimeTrackerData value)
        {
            string StartTime = value.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
            string EndTime = value.EndTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");

            if (value.Category == null)
            {
                return string.Format("{0},{1}", StartTime, EndTime);
            } else {
                string Category = value.Category.Name;
                return string.Format("{0},{1},{2}", StartTime, EndTime, Category);
            }

        }

        /// <summary>
        /// Deserielizes a single Time Tracker Data Value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="categoryMaxLength">Maximum length for Category</param>
        /// <returns></returns>
        public static TimeTrackerData DeserializeValue(string value, int categoryMaxLength)
        {
            var pieces = value.Split(',');

            if (pieces.Length == 2)
            {
                return new TimeTrackerData(DateTimeOffset.Parse(pieces[0]), DateTimeOffset.Parse(pieces[1]));
            }

            if (pieces.Length == 3)
            {
                string category = pieces[2];
                if (category.Length > categoryMaxLength)
                {
                    category = category.Substring(0, categoryMaxLength);
                }

                return new TimeTrackerData(DateTimeOffset.Parse(pieces[0]), DateTimeOffset.Parse(pieces[1]), new TrackedDataCategory(category));
            }

            throw new DeserializationException("Encountered invalid serialized value");
        }
    }

    /// <summary>
    /// Deserialization Exception
    /// </summary>
    public class DeserializationException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public DeserializationException(string message) : base(message)
        {
        }
    }
}


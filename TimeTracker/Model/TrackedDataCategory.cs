using System;

namespace TimeTracker.Model
{
    /// <summary>
    /// Represents a time tracker data category
    /// </summary>
    public class TrackedDataCategory
    {
        /// <summary>
        /// Category name
        /// </summary>
        public String Name;

        /// <summary>
        /// Creates a category, setting its name
        /// </summary>
        /// <param name="name"></param>
        public TrackedDataCategory(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Custom equality comparison implementation
        /// 
        /// Categories are considered equal when their names match
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            return other is TrackedDataCategory ? this.Name == ((TrackedDataCategory) other).Name : false;
        }

        /// <summary>
        /// Custom HashCode implementation
        /// 
        /// Categories return their Name's HashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        /// <summary>
        /// Returns the category name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
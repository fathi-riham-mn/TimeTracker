namespace System.Reflection
{
    using System;

    /// <summary>
    /// Represents the Source Code Link Assembly Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblySourceLinkAttribute : Attribute
    {
        private Uri m_sourceLink;

        /// <summary>
        /// Source Code Assembly Attribute Contructor
        /// </summary>
        /// <param name="link">The source code URI</param>
        public AssemblySourceLinkAttribute(String link)
        {
            m_sourceLink = new Uri(link);
        }

        /// <summary>
        /// Returns the license URI
        /// </summary>
        public Uri SourceLink
        {
            get { return m_sourceLink; }
        }
    }
}
namespace System.Reflection
{
    using System;

    /// <summary>
    /// Represents the License Assembly Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyLicenseAttribute : Attribute
    {
        private String m_license;

        /// <summary>
        /// License Assembly Attribute Contructor
        /// </summary>
        /// <param name="license">The license text</param>
        public AssemblyLicenseAttribute(String license)
        {
            m_license = license;
        }

        /// <summary>
        /// Returns the license text
        /// </summary>
        public String License
        {
            get { return m_license; }
        }
    }
}

using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace TimeTracker.Form
{
    partial class AboutBox : System.Windows.Forms.Form
    {
        public AboutBox()
        {
            InitializeComponent();
            this.Text = String.Format(this.Text, AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format(this.labelVersion.Text, AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.textBoxDescription.Text = AssemblyDescription;
            this.textBoxLicense.Text = AssemblyLicense;

            this.sourceCodeLink.Text = AssemblySourceLink.ToString();
            this.sourceCodeLink.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkClicked);
        }

        /// <summary>
        /// Opens the source code link when clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo(AssemblySourceLink.ToString());
            Process.Start(processInfo);
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyLicense
        {
            get
            {
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyLicenseAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyLicenseAttribute)attributes[0]).License;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public Uri AssemblySourceLink
        {
            get
            {
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblySourceLinkAttribute), false);
                if (attributes.Length == 0)
                {
                    return null;
                }
                return ((AssemblySourceLinkAttribute)attributes[0]).SourceLink;
            }
        }

        #endregion
    }
}
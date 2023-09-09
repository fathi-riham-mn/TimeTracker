using System;
using System.Globalization;
using TimeTracker.Form;
using TimeTracker.Properties;

namespace TimeTracker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (Settings.Default.language != "none")
            {
                try
                {
                    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo(Settings.Default.language);
                    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(Settings.Default.language);
                }
                catch (CultureNotFoundException)
                {
                    System.Windows.Forms.MessageBox.Show(string.Format("Error: invalid culture (language) '{0}'", Settings.Default.language), "Critical error!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
                }
            }

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new Application());
        }
    }
}
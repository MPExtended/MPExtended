using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WindowPlugins.GUITVSeries;

namespace MPExtended.Scrapers.MPTvSeries.Config
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ConfigurationForm form = new ConfigurationForm();
            form.TabControlSettings.TabPages.RemoveAt(3);//follw.it
            form.TabControlSettings.TabPages.RemoveAt(3);//General
            form.TabControlSettings.TabPages.RemoveAt(3);//views/filters
            form.TabControlSettings.TabPages.RemoveAt(3);//layout
            Application.Run(form);

            
        }
    }
}

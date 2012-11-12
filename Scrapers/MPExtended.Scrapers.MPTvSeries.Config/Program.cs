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
            TabControl tabs = (TabControl)form.Controls[0].Controls[0].Controls[0];
            tabs.TabPages.RemoveAt(3);//follw.it
            tabs.TabPages.RemoveAt(3);//General
            tabs.TabPages.RemoveAt(3);//views/filters
            tabs.TabPages.RemoveAt(3);//layout
            Application.Run(form);

            
        }
    }
}

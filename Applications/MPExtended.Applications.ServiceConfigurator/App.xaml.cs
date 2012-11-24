#region Copyright (C) 2011-2012 MPExtended
// Copyright (C) 2011-2012 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Strings;

namespace MPExtended.Applications.ServiceConfigurator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex = new Mutex(true, "{a7a5c7b7-4fa1-4e79-9568-634ff895afe4}");

        private void OnStartup(object sender, StartupEventArgs e)
        {
            // setup
            Log.Setup("ServiceConfigurator.log", false);
            Installation.Load(MPExtendedProduct.Service);

            // make sure to start only once
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                UserServices.Setup(host: false);
                UserServices.Private.OpenConfigurator();
                this.Shutdown(0);
                return;
            }

            // defaults
            StartupArguments.RunAsTrayApp = true;
            StartupArguments.OpenOnStart = true;

            // parse command line arguments
            foreach (string arg in e.Args)
            {
                switch (arg)
                {
                    case "/OnlyTray":
                    case "/OnBoot":
                        StartupArguments.RunAsTrayApp = true;
                        StartupArguments.OpenOnStart = false;
                        break;
                    case "/NoTray":
                    case "/OnlyConfigurator":
                        StartupArguments.RunAsTrayApp = false;
                        StartupArguments.OpenOnStart = true;
                        break;
                    default:
                        Log.Warn("Unknown command-line parameter {0}", arg);
                        break;
                }
            }

            if (Installation.GetFileLayoutType() == FileLayoutType.Installed)
            {
                // change to installation directory
                Environment.CurrentDirectory = Installation.GetInstallDirectory();
            }

            // Load the configuration:
            // - This is needed to make sure Authentication.xml is created before Services.xml is upgraded. 
            // - This makes sure all our configuration files are valid. 
            Configuration.Load();

            // set language
            if (Configuration.Services.DefaultLanguage != null)
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(Configuration.Services.DefaultLanguage);
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(Configuration.Services.DefaultLanguage);
            }

            // set startup form
            this.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
        }

        private void LogErrorAndWarnUser(Exception e)
        {
            Log.Fatal("Possibly fatal error occured", e);
            MessageBox.Show(UI.UnexpectedError, "MPExtended", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogErrorAndWarnUser(e.Exception);
            e.Handled = false;
        }
    }
}

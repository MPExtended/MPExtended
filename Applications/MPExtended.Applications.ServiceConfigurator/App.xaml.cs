#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
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
using MPExtended.Applications.ServiceConfigurator.Code;
using MPExtended.Libraries.General;

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
            // make sure to start only once
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                UserServices.Private.OpenConfigurator();
                this.Shutdown(0);
                return;
            }

            // defaults
            StartupArguments.RunAsTrayApp = true;
            StartupArguments.OpenOnStart = false;

            // parse command line arguments
            foreach (string arg in e.Args)
            {
                switch (arg)
                {
                    case "/Tray":
                        StartupArguments.RunAsTrayApp = true;
                        break;
                    case "/NoTray":
                    case "/OnlyConfigurator":
                        StartupArguments.RunAsTrayApp = false;
                        StartupArguments.OpenOnStart = true;
                        break;
                    case "/Open":
                        StartupArguments.OpenOnStart = true;
                        break;
                    default:
                        Log.Warn("Unknown command-line parameter {0}", arg);
                        break;
                }
            }

#if !DEBUG
            // change to installation directory
            Environment.CurrentDirectory = Installation.GetInstallDirectory(MPExtendedProduct.Service);
#endif

            // set startup form
            this.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
        }
    }
}

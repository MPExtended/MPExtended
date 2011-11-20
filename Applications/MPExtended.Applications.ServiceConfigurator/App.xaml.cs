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
using System.Linq;
using System.Windows;
using MPExtended.Libraries.General;
using MPExtended.Applications.ServiceConfigurator.Code;

namespace MPExtended.Applications.ServiceConfigurator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            // defaults
            StartupArguments.RunAsTrayApp = true;

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
                        break;
                    default:
                        Log.Warn("Unknown command-line parameter {0}", arg);
                        break;
                }
            }
        }
    }
}

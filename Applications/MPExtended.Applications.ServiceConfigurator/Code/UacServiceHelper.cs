﻿#region Copyright (C) 2011-2012 MPExtended
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Windows;
using MPExtended.Libraries.Service;
using MPExtended.Libraries.Service.Strings;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal class UacServiceHelper
    {
        public static bool IsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool StartService()
        {
            return RunUacServiceHandler("/command:service /action:start");
        }

        public static bool StopService()
        {
            return RunUacServiceHandler("/command:service /action:stop");
        }

        public static bool RestartService()
        {
            return RunUacServiceHandler("/command:service /action:restart");
        }

        public static bool RunUacServiceHandler(string parameters)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo();

                if (Installation.GetFileLayoutType() == FileLayoutType.Source)
                {
                    info.FileName = Path.Combine(Installation.GetSourceRootDirectory(),
                        "Applications", "MPExtended.Applications.UacServiceHandler", "bin", Installation.GetSourceBuildDirectoryName(), "MPExtended.Applications.UacServiceHandler.exe");
                }
                else
                {
                    info.FileName = Path.Combine(Installation.GetInstallDirectory(), "MPExtended.Applications.UacServiceHandler.exe");
                }

                info.UseShellExecute = true;
                info.Verb = "runas"; // Provides Run as Administrator
                info.Arguments = parameters;

                if (Process.Start(info) == null)
                {
                    // The user didn't accept the UAC prompt.
                    MessageBox.Show(UI.ActionNeedsAdmin, "MPExtended", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error starting UacServiceHandler", ex);
                return false;
            }
            return true;
        }
    }
}

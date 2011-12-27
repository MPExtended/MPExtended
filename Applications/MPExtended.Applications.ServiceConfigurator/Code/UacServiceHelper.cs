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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using MPExtended.Libraries.General;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal class UacServiceHelper
    {
        public static bool StartService()
        {
            return RunUacServiceHandler("start");
        }

        public static bool StopService()
        {
            return RunUacServiceHandler("stop");
        }

        public static bool RestartService()
        {
            return RunUacServiceHandler("restart");
        }

        private static bool RunUacServiceHandler(String _parameters)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo();

                if (Installation.GetFileLayoutType() == FileLayoutType.Source)
                {
                    info.FileName = Path.Combine(Installation.GetSourceRootDirectory(),
                        "Applications", "MPExtended.Applications.UacServiceHandler", "bin", "Debug", "MPExtended.Applications.UacServiceHandler.exe");
                }
                else
                {

                    info.FileName = Path.Combine(Installation.GetInstallDirectory(MPExtendedProduct.Service), "MPExtended.Applications.UacServiceHandler.exe");
                }

                info.UseShellExecute = true;
                info.Verb = "runas"; // Provides Run as Administrator
                info.Arguments = _parameters;

                if (Process.Start(info) == null)
                {
                    // The user didn't accept the UAC prompt.
                    MessageBox.Show("This action needs administrative rights.", "MPExtended", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error starting uac service handler", ex);
                return false;
            }
            return true;
        }
    }
}

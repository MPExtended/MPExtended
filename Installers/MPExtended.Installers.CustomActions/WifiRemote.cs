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
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;

namespace MPExtended.Installers.CustomActions
{
    internal static class WifiRemote
    {
        public static bool Install(Session session)
        {
            
            // get wifiremote MPEI package
            string tempFile = Path.GetTempFileName();
            if (!BinaryData.ExtractToFile(session, "WifiRemoteInstallerBin", tempFile))
            {
                return false;
            }
            session.Log("WifiRemote: extracted WifiRemote to {0}", tempFile);

            // lookup MPEI installer location
            string mpei = LookupMPEI(session);
            if (mpei == null)
            {
                return false;
            }
            session.Log("WifiRemote: found MPEI at {0}", mpei);

            // execute
            ProcessStartInfo param = new ProcessStartInfo();
            param.FileName = mpei;
            param.Arguments = tempFile + " /S";
            session.Log("WifiRemote: starting MPEI with arguments {0}", param.Arguments);
            Process proc = new Process();
            proc.StartInfo = param;
            proc.Start();
            proc.WaitForExit();
            session.Log("WifiRemote: MPEI finished with exit code {0}", proc.ExitCode);

            // cleanup
            File.Delete(tempFile);
            return true;
        }

        private static string LookupMPEI(Session session)
        {
            try
            {
                string keyPath = Environment.Is64BitOperatingSystem ?
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal" :
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
                if (key == null)
                {
                    session.Log("WifiRemote: Could not find MediaPortal installation path key");
                    return null;
                }

                object value = key.GetValue("InstallPath", null);
                if (value == null)
                {
                    session.Log("WifiRemote: Could not find MediaPortal installation path value");
                    return null;
                }

                string path = value.ToString();
                string mpei = Path.Combine(path, "MpeInstaller.exe");
                if (!File.Exists(mpei))
                {
                    session.Log("WifiRemote: MPEI path {0} does not exists", mpei);
                    return null;
                }

                return mpei;
            }
            catch (Exception ex)
            {
                session.Log("WifiRemote: Failed to lookup MPEI path", ex);
                return null;
            }
        }
    }
}

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
using System.Net;
using System.Text;
using System.Xml.Linq;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;

namespace MPExtended.Installers.CustomActions
{
    internal static class WifiRemote
    {
        private const string UPDATE_FILE = @"http://wifiremote.googlecode.com/svn/trunk/Installer/update.xml";

        public static bool Install(Session session)
        {
            // download WifiRemote MPEI package
            session.Message(InstallMessage.ActionStart, new Record("callAddProgressInfo", "Downloading WifiRemote installer...", "xxx"));
            string tempFile = Path.GetTempFileName();
            if (!DownloadWifiRemote(tempFile))
            {
                if(File.Exists(tempFile)) 
                {
                    File.Delete(tempFile);
                }

                Log.Write("WifiRemote: failed to download it, try packaged version");
                if (!BinaryData.ExtractToFile(session, "WifiRemoteInstallerBin", tempFile))
                {
                    Log.Write("WifiRemote: extracting packaged version also failed, giving up");
                    return false;
                }
            }
            Log.Write("WifiRemote: extracted WifiRemote to {0}", tempFile);

            // lookup MPEI installer location
            session.Message(InstallMessage.ActionStart, new Record("callAddProgressInfo", "Installing WifiRemote MediaPortal plugin with MPEI...", "xxx"));
            string mpei = LookupMPEI();
            if (mpei == null)
            {
                return false;
            }
            Log.Write("WifiRemote: found MPEI at {0}", mpei);

            // execute
            ProcessStartInfo param = new ProcessStartInfo();
            param.FileName = mpei;
            param.Arguments = tempFile + " /S";
            Log.Write("WifiRemote: starting MPEI with arguments {0}", param.Arguments);
            Process proc = new Process();
            proc.StartInfo = param;
            proc.Start();
            proc.WaitForExit();
            Log.Write("WifiRemote: MPEI finished with exit code {0}", proc.ExitCode);

            // cleanup
            File.Delete(tempFile);
            return true;
        }

        private static bool DownloadWifiRemote(string tempPath)
        {
            string xmlData;

            // download update.xml
            try
            {
                Log.Write("WifiRemote: Downloading update.xml from {0}", UPDATE_FILE);
                using (WebClient client = new WebClient())
                {
                    xmlData = client.DownloadString(UPDATE_FILE);
                }
            }
            catch (Exception ex)
            {
                Log.Write("WifiRemote: Failed to download update.xml: {0}", ex.Message);
                return false;
            }

            // parse it
            Uri file;
            try
            {
                XElement updateFile = XElement.Parse(xmlData);
                string uri = updateFile.Element("Items").Element("PackageClass").Element("GeneralInfo").Element("OnlineLocation").Value.ToString();
                file = new Uri(uri);
            }
            catch (Exception ex)
            {
                Log.Write("WifiRemote: Failed to parse update.xml: {0}", ex.Message);
                return false;
            }

            // download it
            try
            {
                Log.Write("WifiRemote: Downloading from {0}", file.ToString());
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(file, tempPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write("WifiRemote: Failed to download WifiRemote", ex.Message);
                return false;
            }
        }

        private static string LookupMPEI()
        {
            try
            {
                string keyPath = Environment.Is64BitOperatingSystem ?
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal" :
                    @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
                if (key == null)
                {
                    Log.Write("WifiRemote: Could not find MediaPortal installation path key");
                    return null;
                }

                object value = key.GetValue("InstallPath", null);
                if (value == null)
                {
                    Log.Write("WifiRemote: Could not find MediaPortal installation path value");
                    return null;
                }

                string path = value.ToString();
                string mpei = Path.Combine(path, "MpeInstaller.exe");
                if (!File.Exists(mpei))
                {
                    Log.Write("WifiRemote: MPEI path {0} does not exists", mpei);
                    return null;
                }

                return mpei;
            }
            catch (Exception ex)
            {
                Log.Write("WifiRemote: Failed to lookup MPEI path", ex);
                return null;
            }
        }
    }
}
